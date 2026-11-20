<script setup lang="ts">
import {mdiCheck} from '@mdi/js';
import {computed, onMounted, onUnmounted, ref, watch} from 'vue';
import {onBeforeRouteLeave} from 'vue-router';
import Icon from '../components/Icon.vue';
import MoneyField from '../components/MoneyField.vue';
import SaveBar from '../components/SaveBar.vue';
import SegmentedControl from '../components/SegmentedControl.vue';
import TariffBadge from '../components/TariffBadge.vue';
import ToggleSwitch from '../components/ToggleSwitch.vue';
import {useAccount} from '../composables/useAccount';
import {useLive} from '../composables/useLive';
import {useSettings} from '../composables/useSettings';
import {ApiError, get, type Settings} from '../lib/api';
import {num, time} from '../lib/format';
import {getTheme, setTheme, type ThemeChoice} from '../lib/theme';

const {settings, save: saveSettings} = useSettings();
const {account} = useAccount();
const {created, status} = useLive();

// Form = settings + theme (theme lives in localStorage, but is saved with the same SaveBar)
type Form = Settings & { theme: ThemeChoice };
const saved = ref<Form | null>(null);
const form = ref<Form | null>(null);
const formKey = ref(0); // re-creates the fields on Herstellen, clearing half-typed input
const saving = ref(false);
const justSaved = ref(false);
const saveError = ref('');

watch(settings, s => {
  if (s && !saved.value) {
    saved.value = {...s, theme: getTheme()};
    form.value = {...saved.value};
  }
}, {immediate: true});

// Theme previews right away
watch(() => form.value?.theme, t => t && setTheme(t, false));

const fields: { key: keyof Form; label: string }[] = [
  {key: 'solarSystem', label: 'Systeem met zonnepanelen'}, {key: 'showDayName', label: 'Dagnaam in grafieken'},
  {key: 'theme', label: 'Thema'}, {key: 'timeZoneId', label: 'Tijdzone'}, {key: 'postcode', label: 'Postcode'},
  {key: 'highUsagePricePerKwh', label: 'Opname normaaltarief'}, {key: 'lowUsagePricePerKwh', label: 'Opname daltarief'},
  {key: 'highRedeliveryPricePerKwh', label: 'Teruglevering normaaltarief'}, {key: 'lowRedeliveryPricePerKwh', label: 'Teruglevering daltarief'},
  {key: 'gasPrice', label: 'Gasprijs'}, {key: 'electricityDeliveryPricePerMonth', label: 'Vaste kosten stroom'},
  {key: 'gasDeliveryPricePerMonth', label: 'Vaste kosten gas'},
];
const changed = computed(() => fields.filter(f => form.value && saved.value && (form.value[f.key] ?? '') !== (saved.value[f.key] ?? '')));
const isChanged = (key: keyof Form) => changed.value.some(f => f.key === key);
const changeNote = (key: keyof Form, digits = 4) => {
  const f = fields.find(x => x.key === key)!;
  return `${f.label} gewijzigd, was € ${num(saved.value![key] as number, digits)}.`;
};

async function save() {
  if (!form.value) return;
  saving.value = true;
  saveError.value = '';
  try {
    const {theme, ...next} = form.value;
    await saveSettings(next);
    setTheme(theme);
    saved.value = {...settings.value!, theme};
    form.value = {...saved.value};
    justSaved.value = true;
    setTimeout(() => (justSaved.value = false), 4000);
  } catch (e) {
    // 400s carry a Dutch message (e.g. unknown postcode)
    saveError.value = e instanceof ApiError && e.status === 400 ? e.message : 'Opslaan is niet gelukt. Probeer het opnieuw.';
  } finally {
    saving.value = false;
  }
}

function reset() {
  form.value = {...saved.value!};
  saveError.value = '';
  formKey.value++;
}

// Ask before leaving with unsaved changes
onBeforeRouteLeave(() => changed.value.length === 0 || confirm('Je wijzigingen zijn nog niet opgeslagen. Toch weggaan?'));
const beforeUnload = (e: BeforeUnloadEvent) => {
  if (changed.value.length) e.preventDefault();
};
onMounted(() => window.addEventListener('beforeunload', beforeUnload));
onUnmounted(() => {
  window.removeEventListener('beforeunload', beforeUnload);
  setTheme(getTheme(), false); // drop an unsaved theme preview
});

// Time zones, grouped by region: "Amsterdam (UTC+1)"
const zones = ref<{ region: string; items: { id: string; label: string }[] }[]>([]);
get<{ timeZones: { id: string }[] }>('/webapi/v3/settings/time-zones').then(r => {
  const groups = new Map<string, { id: string; label: string }[]>();
  for (const {id} of r?.timeZones ?? []) {
    const [region, ...rest] = id.split('/');
    if (!rest.length || region === 'Etc' || region === 'SystemV') continue;
    let offset = '';
    try {
      offset = new Intl.DateTimeFormat('en', {timeZone: id, timeZoneName: 'shortOffset'}).formatToParts(new Date())
          .find(p => p.type === 'timeZoneName')?.value.replace('GMT', 'UTC') ?? '';
    } catch { continue; }
    if (offset === 'UTC') offset = 'UTC+0';
    const list = groups.get(region) ?? [];
    list.push({id, label: `${rest.join(' / ').replace(/_/g, ' ')} (${offset})`});
    groups.set(region, list);
  }
  zones.value = [...groups].sort(([a], [b]) => a.localeCompare(b))
      .map(([region, items]) => ({region, items: items.sort((a, b) => a.label.localeCompare(b.label))}));
}).catch(() => undefined);

const showKey = ref(false);
const themes = [{value: 'licht' as const, label: 'Licht'}, {value: 'donker' as const, label: 'Donker'}, {value: 'systeem' as const, label: 'Systeem'}];
const tz = computed(() => saved.value?.timeZoneId ?? 'Europe/Amsterdam');
</script>

<template>
  <h1 class="ez-title-lg">Instellingen</h1>

  <div class="mt-6 flex gap-8">
    <nav aria-label="Onderdelen" class="w-[200px] shrink-0 max-md:hidden">
      <ul class="sticky top-20 m-0 flex list-none flex-col gap-1 p-0">
        <li v-for="s in [['algemeen', 'Algemeen'], ['tarieven', 'Tarieven'], ['account', 'Account']]" :key="s[0]">
          <a :href="`#${s[0]}`" class="block rounded-md px-3 py-2 text-ink no-underline hover:bg-brand-soft">{{ s[1] }}</a>
        </li>
      </ul>
    </nav>

    <div v-if="!form" class="flex max-w-[760px] flex-1 flex-col gap-5">
      <div class="ez-card"><div class="ez-skel h-72"/></div>
      <div class="ez-card"><div class="ez-skel h-96"/></div>
    </div>

    <div v-else :key="`form-${formKey}`" class="flex min-w-0 max-w-[760px] flex-1 flex-col gap-5 max-[560px]:gap-4">
      <!-- Algemeen -->
      <section id="algemeen" class="ez-card scroll-mt-20" aria-labelledby="algemeen-title">
        <h2 id="algemeen-title" class="ez-title border-b border-line pb-3">Algemeen</h2>
        <div class="divide-y divide-line">
          <div class="flex items-center justify-between gap-4 py-4">
            <div>
              <p id="solar-label" class="ez-body-strong m-0">Systeem met zonnepanelen</p>
              <p id="solar-desc" class="ez-caption m-0">Toont Opwekking en Teruglevering op Actueel en Historie. Uit: je ziet alleen Verbruik en Opname.</p>
            </div>
            <ToggleSwitch v-model="form.solarSystem" labelledby="solar-label" describedby="solar-desc"/>
          </div>
          <div class="flex items-center justify-between gap-4 py-4">
            <div>
              <p id="dayname-label" class="ez-body-strong m-0">Dagnaam in grafieken</p>
              <p id="dayname-desc" class="ez-caption m-0">Bijvoorbeeld ‘do 19 nov’ in plaats van ‘19 nov’.</p>
            </div>
            <ToggleSwitch v-model="form.showDayName" labelledby="dayname-label" describedby="dayname-desc"/>
          </div>
          <div class="flex flex-wrap items-center justify-between gap-4 py-4">
            <div>
              <p class="ez-body-strong m-0">Thema</p>
              <p class="ez-caption m-0">Systeem volgt de instelling van je apparaat. Geldt alleen voor dit apparaat.</p>
            </div>
            <SegmentedControl v-model="form.theme" :options="themes" label="Thema"/>
          </div>
          <div class="flex flex-wrap items-center justify-between gap-4 py-4">
            <div>
              <label for="tz" class="ez-body-strong m-0 block">Tijdzone</label>
              <p class="ez-caption m-0">Bepaalt waar een dag begint en eindigt.</p>
            </div>
            <select id="tz" v-model="form.timeZoneId" class="ez-input w-[280px] max-[560px]:w-full">
              <option v-if="!zones.length" :value="form.timeZoneId">{{ form.timeZoneId }}</option>
              <optgroup v-for="g in zones" :key="g.region" :label="g.region">
                <option v-for="z in g.items" :key="z.id" :value="z.id">{{ z.label }}</option>
              </optgroup>
            </select>
          </div>
          <div class="flex flex-wrap items-center justify-between gap-4 pt-4">
            <div>
              <label for="postcode" class="ez-body-strong m-0 block">Postcode</label>
              <p id="postcode-desc" class="ez-caption m-0">
                Voor de buitentemperatuur bij Gas op Historie.
                <template v-if="saved?.weatherStation && form.postcode === saved.postcode">Weerstation: {{ saved.weatherStation }} (KNMI).</template>
              </p>
            </div>
            <input id="postcode" v-model.trim="form.postcode" class="ez-input w-[280px] max-[560px]:w-full" autocomplete="postal-code"
                   placeholder="1234 AB" maxlength="7" aria-describedby="postcode-desc">
          </div>
        </div>
      </section>

      <!-- Tarieven -->
      <section id="tarieven" class="ez-card scroll-mt-20" aria-labelledby="tarieven-title">
        <h2 id="tarieven-title" class="ez-title">Tarieven</h2>
        <p class="mt-1 mb-0 text-ink-muted">Hiermee rekent EnergieZicht je kosten uit. Vul de bedragen in zoals op je contract, inclusief btw.</p>

        <p class="ez-label mt-5 mb-3">Stroom per kWh</p>
        <div class="grid grid-cols-[auto_1fr_1fr] items-center gap-x-4 gap-y-3 max-[560px]:grid-cols-1">
          <span class="max-[560px]:hidden"/>
          <span id="col-normaal" class="max-[560px]:hidden"><TariffBadge :mode="2"/></span>
          <span id="col-dal" class="max-[560px]:hidden"><TariffBadge :mode="1"/></span>

          <span id="row-opname" class="ez-body-strong">Opname</span>
          <label class="contents">
            <span class="ez-caption hidden max-[560px]:block">Normaaltarief</span>
            <MoneyField id="hu" v-model="form.highUsagePricePerKwh" unit="/kWh" labelledby="row-opname col-normaal"
                        :state="isChanged('highUsagePricePerKwh') ? 'changed' : 'normal'"/>
          </label>
          <label class="contents">
            <span class="ez-caption hidden max-[560px]:block">Daltarief</span>
            <MoneyField id="lu" v-model="form.lowUsagePricePerKwh" unit="/kWh" labelledby="row-opname col-dal"
                        :state="isChanged('lowUsagePricePerKwh') ? 'changed' : 'normal'"/>
          </label>

          <span id="row-terug" class="ez-body-strong">Teruglevering</span>
          <label class="contents">
            <span class="ez-caption hidden max-[560px]:block">Normaaltarief</span>
            <MoneyField id="hr" v-model="form.highRedeliveryPricePerKwh" unit="/kWh" allow-negative labelledby="row-terug col-normaal"
                        :state="isChanged('highRedeliveryPricePerKwh') ? 'changed' : 'normal'"/>
          </label>
          <label class="contents">
            <span class="ez-caption hidden max-[560px]:block">Daltarief</span>
            <MoneyField id="lr" v-model="form.lowRedeliveryPricePerKwh" unit="/kWh" allow-negative labelledby="row-terug col-dal"
                        :state="isChanged('lowRedeliveryPricePerKwh') ? 'changed' : 'normal'"/>
          </label>
        </div>
        <p v-for="k in (['highUsagePricePerKwh', 'lowUsagePricePerKwh', 'highRedeliveryPricePerKwh', 'lowRedeliveryPricePerKwh'] as const).filter(isChanged)"
           :key="k" class="ez-caption mt-2 mb-0">{{ changeNote(k) }}</p>
        <p class="ez-caption mt-2 mb-0">Teruglevering mag negatief zijn als je contract een negatieve terugleververgoeding heeft.</p>

        <div class="mt-5 grid grid-cols-2 gap-x-4 gap-y-4 border-t border-line pt-5 max-[560px]:grid-cols-1">
          <div class="col-span-2 max-w-[calc(50%-8px)] max-[560px]:col-span-1 max-[560px]:max-w-none">
            <label for="gas" class="ez-label mb-1.5 block">Gas</label>
            <MoneyField id="gas" v-model="form.gasPrice" unit="/m³" describedby="gas-note"
                        :state="isChanged('gasPrice') ? 'changed' : !form.gasPrice ? 'warning' : 'normal'"/>
            <p v-if="isChanged('gasPrice')" id="gas-note" class="ez-caption mt-1 mb-0">{{ changeNote('gasPrice') }}</p>
            <p v-else-if="!form.gasPrice" id="gas-note" class="ez-caption mt-1 mb-0 !text-warning">Nog niet ingesteld. Zonder gasprijs zie je geen gaskosten.</p>
          </div>
          <div>
            <label for="fe" class="ez-label mb-1.5 block">Vaste kosten stroom</label>
            <MoneyField id="fe" v-model="form.electricityDeliveryPricePerMonth" unit="/maand" :digits="2"
                        :state="isChanged('electricityDeliveryPricePerMonth') ? 'changed' : 'normal'"/>
            <p v-if="isChanged('electricityDeliveryPricePerMonth')" class="ez-caption mt-1 mb-0">{{ changeNote('electricityDeliveryPricePerMonth', 2) }}</p>
          </div>
          <div>
            <label for="fg" class="ez-label mb-1.5 block">Vaste kosten gas</label>
            <MoneyField id="fg" v-model="form.gasDeliveryPricePerMonth" unit="/maand" :digits="2"
                        :state="isChanged('gasDeliveryPricePerMonth') ? 'changed' : 'normal'"/>
            <p v-if="isChanged('gasDeliveryPricePerMonth')" class="ez-caption mt-1 mb-0">{{ changeNote('gasDeliveryPricePerMonth', 2) }}</p>
          </div>
        </div>
      </section>

      <!-- Account -->
      <section id="account" class="ez-card scroll-mt-20" aria-labelledby="account-title">
        <h2 id="account-title" class="ez-title border-b border-line pb-3">Account</h2>
        <div class="divide-y divide-line">
          <div class="flex items-center justify-between gap-4 py-4">
            <div class="min-w-0">
              <p class="ez-body-strong m-0">E-mailadres</p>
              <p class="m-0 truncate text-ink-muted">{{ account?.email ?? '…' }}</p>
            </div>
            <a class="ez-link" href="/Identity/Account/Manage#e-mailadres">Wijzigen</a>
          </div>
          <div class="flex items-center justify-between gap-4 py-4">
            <div>
              <p class="ez-body-strong m-0">Wachtwoord</p>
              <p class="m-0 text-ink-muted">Wijzig je wachtwoord op de accountpagina.</p>
            </div>
            <a class="ez-link" href="/Identity/Account/Manage#wachtwoord">Wijzigen</a>
          </div>
          <div class="flex items-center justify-between gap-4 py-4">
            <div>
              <p class="ez-body-strong m-0">Tweestapsverificatie</p>
              <p v-if="account?.twoFactorEnabled" class="m-0 flex items-center gap-1 text-ink-muted"><Icon :path="mdiCheck" :size="16"/>Aan, met een authenticator-app</p>
              <p v-else class="m-0 text-ink-muted">Uit</p>
            </div>
            <a class="ez-link" href="/Identity/Account/Manage#tweestapsverificatie">Beheren</a>
          </div>
          <div class="flex flex-wrap items-center justify-between gap-4 py-4">
            <div class="min-w-0">
              <p class="ez-body-strong m-0">Raspberry Pi</p>
              <p v-if="!account?.rpiKey" class="m-0 text-ink-muted">Nog niet gekoppeld.</p>
              <p v-else-if="created" class="m-0 flex items-center gap-2 text-ink-muted">
                <span class="size-2 rounded-full" :class="status === 'live' ? 'bg-live' : status === 'stale' ? 'bg-warning' : 'bg-danger'"/>
                Gekoppeld, laatste meting {{ time(created, tz, true) }}
              </p>
              <p v-else class="m-0 text-ink-muted">Gekoppeld, nog geen metingen.</p>
              <p v-if="showKey && account?.rpiKey" class="ez-meter mt-2 mb-0 rounded-sm bg-surface-sunken px-2 py-1 break-all">{{ account.rpiKey }}</p>
            </div>
            <button v-if="account?.rpiKey" type="button" class="ez-link bg-transparent p-0" :aria-expanded="showKey" @click="showKey = !showKey">
              {{ showKey ? 'Sleutel verbergen' : 'Sleutel bekijken' }}
            </button>
          </div>
          <div class="pt-4">
            <button type="submit" form="logout-form" class="ez-btn">Uitloggen</button>
          </div>
        </div>
      </section>

      <SaveBar :changes="changed.length" :saving="saving" :saved="justSaved" :error="saveError" @save="save" @reset="reset"/>
    </div>
  </div>
</template>
