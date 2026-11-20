<script setup lang="ts">
import {mdiCalendarRange, mdiCheck, mdiChevronDown, mdiChevronLeft, mdiChevronRight} from '@mdi/js';
import {computed, ref, watch} from 'vue';
import Icon from '../../components/Icon.vue';
import {useDismiss, useMedia} from '../../composables/useUi';
import {addDays, dmy, fromZoned, monthLong, parseDmy, startOfDay, zoned} from '../../lib/format';
import {allowedGroups, describe, GROUP_LABEL, type Grouping, type Period, periodDays, PRESETS, presetPeriod, type Preset} from '../../lib/period';

const props = defineProps<{ timeZone: string }>();
const model = defineModel<Period>({required: true});

const open = ref(false);
const root = ref<HTMLElement | null>(null);
useDismiss(root, open);
const mobile = useMedia('(max-width: 767px)');

const now = () => new Date();
const label = computed(() => describe(model.value, now(), props.timeZone));

// Draft state while the popover is open
const preset = ref<Preset | 'custom'>('7d');
const from = ref<Date | null>(null);   // first day (midnight)
const to = ref<Date | null>(null);     // last day, inclusive (midnight)
const group = ref<Grouping>('days');
const fromText = ref('');
const toText = ref('');
const month = ref({year: 2026, month: 1});

function openPicker() {
  const tz = props.timeZone, p = model.value;
  preset.value = p.preset;
  from.value = startOfDay(p.from, tz);
  to.value = startOfDay(new Date((p.to ?? now()).getTime() - 1), tz);
  group.value = p.group;
  syncText();
  const z = zoned(to.value, tz);
  month.value = {year: z.year, month: z.month};
  open.value = true;
}

function syncText() {
  fromText.value = from.value ? dmy(from.value, props.timeZone) : '';
  toText.value = to.value ? dmy(to.value, props.timeZone) : '';
}

function choosePreset(key: Preset) {
  const p = presetPeriod(key, now(), props.timeZone);
  model.value = p;
  open.value = false;
}

function onText(which: 'from' | 'to') {
  const d = parseDmy(which === 'from' ? fromText.value : toText.value, props.timeZone);
  if (!d || d.getTime() > now().getTime()) return;
  if (which === 'from') from.value = d; else to.value = d;
  preset.value = 'custom';
}

const today = computed(() => startOfDay(now(), props.timeZone).getTime());

/** Calendar cells for the shown month, Monday first. */
const cells = computed(() => {
  const tz = props.timeZone, {year, month: m} = month.value;
  const first = fromZoned(tz, year, m, 1);
  const lead = zoned(first, tz).weekday;
  const days = new Date(Date.UTC(year, m, 0)).getUTCDate();
  const list: ({ date: Date; day: number } | null)[] = Array(lead).fill(null);
  for (let d = 1; d <= days; d++) list.push({date: fromZoned(tz, year, m, d), day: d});
  return list;
});
const monthTitle = computed(() => monthLong(fromZoned(props.timeZone, month.value.year, month.value.month, 15), props.timeZone));

function shiftMonth(delta: number) {
  const d = new Date(Date.UTC(month.value.year, month.value.month - 1 + delta, 1));
  month.value = {year: d.getUTCFullYear(), month: d.getUTCMonth() + 1};
}

function pick(date: Date) {
  preset.value = 'custom';
  if (!from.value || to.value) {
    from.value = date;
    to.value = null;
  } else if (date < from.value) {
    to.value = from.value;
    from.value = date;
  } else {
    to.value = date;
  }
  syncText();
}

const cellState = (date: Date) => {
  const t = date.getTime();
  const a = from.value?.getTime(), b = to.value?.getTime();
  return {
    future: t > today.value,
    end: t === a || t === b,
    inRange: a != null && b != null && t > a && t < b,
  };
};

const draft = computed<Period | null>(() => {
  if (!from.value) return null;
  const last = to.value ?? from.value;
  const end = addDays(last, 1, props.timeZone);
  return {preset: 'custom', from: from.value, to: end.getTime() > now().getTime() ? undefined : end, group: group.value};
});
const allowed = computed(() => (draft.value ? allowedGroups(periodDays(draft.value, now())) : []));
// Keep the grouping valid when the range grows
watch(allowed, a => {
  if (a.length && !a.includes(group.value)) group.value = a[0];
});

function apply() {
  if (preset.value !== 'custom') return choosePreset(preset.value);
  if (draft.value && allowed.value.includes(group.value)) model.value = draft.value;
  open.value = false;
}

const weekdays = ['ma', 'di', 'wo', 'do', 'vr', 'za', 'zo'];
</script>

<template>
  <div ref="root" class="relative">
    <button type="button" class="ez-btn max-w-full !font-normal" :aria-expanded="open" aria-haspopup="dialog" @click="open ? (open = false) : openPicker()">
      <Icon :path="mdiCalendarRange"/>
      <strong class="font-semibold">{{ label.name }}</strong>
      <span class="truncate text-ink-muted">{{ label.detail }}</span>
      <Icon :path="mdiChevronDown"/>
    </button>

    <div v-if="open && mobile" class="fixed inset-0 z-30 bg-scrim" aria-hidden="true" @click="open = false"/>
    <div v-if="open" role="dialog" aria-label="Periode kiezen"
         :class="mobile
           ? 'fixed inset-x-0 bottom-0 z-40 max-h-[90dvh] overflow-y-auto rounded-t-lg bg-surface-raised p-4 pb-[calc(16px+env(safe-area-inset-bottom))] shadow-float'
           : 'absolute left-0 z-30 mt-2 w-[640px] rounded-lg border border-line bg-surface-raised p-5 shadow-float'">
      <div class="grid gap-6" :class="mobile ? 'grid-cols-1' : 'grid-cols-[200px_1fr]'">
        <div>
          <p class="ez-label mt-0 mb-2 text-ink-muted">Snelkeuze</p>
          <ul class="m-0 flex list-none flex-col gap-1 p-0">
            <li v-for="p in PRESETS" :key="p.key">
              <button type="button" class="flex h-10 w-full items-center justify-between rounded-md px-3 text-left text-ink pointer-coarse:h-11"
                      :class="preset === p.key ? 'bg-brand-soft font-semibold' : 'hover:bg-surface-sunken'"
                      :aria-pressed="preset === p.key" @click="choosePreset(p.key)">
                {{ p.label }}<Icon v-if="preset === p.key" :path="mdiCheck" :size="18"/>
              </button>
            </li>
          </ul>
        </div>

        <div>
          <p class="ez-label mt-0 mb-2 text-ink-muted">Aangepast bereik</p>
          <div class="grid grid-cols-2 gap-3">
            <label class="ez-label">Van
              <input v-model="fromText" class="ez-input mt-1 font-normal" placeholder="dd-mm-jjjj" inputmode="numeric" @change="onText('from')">
            </label>
            <label class="ez-label">Tot en met
              <input v-model="toText" class="ez-input mt-1 font-normal" placeholder="dd-mm-jjjj" inputmode="numeric" @change="onText('to')">
            </label>
          </div>

          <div class="mt-4">
            <div class="flex items-center justify-between">
              <button type="button" class="flex size-9 items-center justify-center rounded-md hover:bg-surface-sunken" aria-label="Vorige maand" @click="shiftMonth(-1)">
                <Icon :path="mdiChevronLeft"/>
              </button>
              <span class="ez-body-strong" aria-live="polite">{{ monthTitle }}</span>
              <button type="button" class="flex size-9 items-center justify-center rounded-md hover:bg-surface-sunken" aria-label="Volgende maand" @click="shiftMonth(1)">
                <Icon :path="mdiChevronRight"/>
              </button>
            </div>
            <div class="mt-2 grid grid-cols-7 text-center" role="grid">
              <span v-for="w in weekdays" :key="w" class="ez-caption py-1">{{ w }}</span>
              <template v-for="(c, i) in cells" :key="i">
                <span v-if="!c"/>
                <button v-else type="button" :disabled="cellState(c.date).future"
                        class="h-9 text-sm disabled:cursor-default disabled:text-ink-muted disabled:opacity-50 pointer-coarse:h-11"
                        :class="cellState(c.date).end ? 'rounded-md bg-brand font-semibold text-on-brand'
                          : cellState(c.date).inRange ? 'bg-brand-soft text-ink' : 'rounded-md text-ink hover:bg-surface-sunken'"
                        :aria-pressed="cellState(c.date).end" @click="pick(c.date)">
                  {{ c.day }}
                </button>
              </template>
            </div>
          </div>

          <label class="ez-label mt-4 block">Groeperen per
            <select v-model="group" class="ez-input mt-1 font-normal" aria-describedby="group-help">
              <option v-for="g in (['hours', 'days', 'months', 'years'] as const)" :key="g" :value="g" :disabled="!allowed.includes(g)">
                {{ GROUP_LABEL[g][0].toUpperCase() + GROUP_LABEL[g].slice(1) }}
              </option>
            </select>
          </label>
          <p id="group-help" class="ez-caption mt-1 mb-0">Per uur kan tot 30 dagen, per dag tot 700 dagen.</p>
        </div>
      </div>

      <div class="mt-5 flex justify-end gap-2 border-t border-line pt-4">
        <button type="button" class="ez-btn" :class="mobile ? 'flex-1' : ''" @click="open = false">Annuleren</button>
        <button type="button" class="ez-btn ez-btn-primary" :class="mobile ? 'flex-1' : ''" :disabled="!draft" @click="apply">Toepassen</button>
      </div>
    </div>
  </div>
</template>
