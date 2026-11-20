<script setup lang="ts">
import {computed, onUnmounted, ref} from 'vue';
import Banner from '../components/Banner.vue';
import KpiTile from '../components/KpiTile.vue';
import LiveIndicator from '../components/LiveIndicator.vue';
import TariffBadge from '../components/TariffBadge.vue';
import {useLive} from '../composables/useLive';
import {useSettings} from '../composables/useSettings';
import {get, type DayTotals} from '../lib/api';
import {dayCost, flow, todayFlow} from '../lib/energy';
import {day, meter, num, time} from '../lib/format';
import PowerFlow from './actueel/PowerFlow.vue';
import VermogenChart from './actueel/VermogenChart.vue';

const {settings} = useSettings();
const {reading, created, status} = useLive();
const tz = computed(() => settings.value?.timeZoneId ?? 'Europe/Amsterdam');
const hasSolar = computed(() => settings.value?.solarSystem ?? false);

const today = ref<DayTotals | null>(null);
const yesterday = ref<DayTotals | null>(null);

async function loadToday() {
  const [t, y] = await Promise.allSettled([
    get<DayTotals>('/webapi/v3/energy/total-today'),
    get<DayTotals>('/webapi/v3/energy/total-yesterday-until-now'),
  ]);
  if (t.status === 'fulfilled') today.value = t.value;
  // No comparison bars when yesterday is unknown, instead of faking them
  yesterday.value = y.status === 'fulfilled' ? y.value : null;
}
void loadToday();
const todayTimer = window.setInterval(loadToday, 60_000);
onUnmounted(() => clearInterval(todayTimer));

const now = computed(() => {
  const r = reading.value;
  return r && settings.value ? flow(r.usageNow / 1000, r.solarNow / 1000, r.redeliveryNow / 1000, hasSolar.value) : null;
});

const kpi = computed(() => {
  const s = settings.value;
  if (!today.value || !s) return null;
  const t = todayFlow(today.value, hasSolar.value);
  const y = yesterday.value ? todayFlow(yesterday.value, hasSolar.value) : null;
  return {
    t: {...t, kosten: dayCost(today.value, s)},
    y: y && yesterday.value ? {...y, kosten: dayCost(yesterday.value, s)} : null,
  };
});

const one = (v: number) => num(v, 1);
const two = (v: number) => num(v, 2);

const meters = computed(() => {
  const r = reading.value;
  if (!r) return [];
  const rows = [
    {label: 'Opname normaal', value: r.usageTotalHigh, unit: 'kWh'},
    {label: 'Opname dal', value: r.usageTotalLow, unit: 'kWh'},
  ];
  if (hasSolar.value) rows.push(
      {label: 'Teruglevering normaal', value: r.redeliveryTotalHigh, unit: 'kWh'},
      {label: 'Teruglevering dal', value: r.redeliveryTotalLow, unit: 'kWh'},
      {label: 'Opwekking totaal', value: r.solarTotal, unit: 'kWh'});
  // API values are Wh and dm³
  rows.push({label: 'Gas', value: r.usageGasTotal, unit: 'm³'});
  return rows.map(row => ({...row, text: meter(row.value / 1000)}));
});
</script>

<template>
  <div class="flex flex-col gap-5 max-[560px]:gap-4">
    <div class="flex items-baseline gap-3">
      <h1 class="ez-title-lg">Actueel</h1>
      <span class="ez-caption text-[15px]">{{ day(new Date(), tz) }}</span>
    </div>

    <Banner v-if="status === 'offline' && created" variant="danger">
      <strong>Geen nieuwe metingen sinds {{ time(created, tz) }}.</strong> Controleer de Raspberry Pi in de meterkast.
    </Banner>
    <Banner v-else-if="status === 'none'" variant="warning">
      <strong>Nog geen metingen ontvangen.</strong> Controleer of de Raspberry Pi in de meterkast aan staat en gekoppeld is.
    </Banner>

    <!-- Nu -->
    <section class="ez-card" aria-labelledby="nu-title">
      <div class="flex flex-wrap items-center gap-3">
        <h2 id="nu-title" class="ez-title">Nu</h2>
        <TariffBadge v-if="reading" :mode="reading.mode"/>
        <span class="ml-auto"><LiveIndicator :status="status" :created="created" :time-zone="tz"/></span>
      </div>
      <div class="mt-5">
        <PowerFlow :flow="settings ? now : null" :has-solar="hasSolar"/>
      </div>
    </section>

    <!-- Vandaag -->
    <section class="ez-card" aria-labelledby="vandaag-title">
      <div class="flex flex-wrap items-baseline gap-x-3">
        <h2 id="vandaag-title" class="ez-title">Vandaag</h2>
        <span class="ez-caption">tot {{ time(new Date(), tz) }}<template v-if="kpi?.y"> · de streep is gisteren tot hetzelfde tijdstip</template></span>
      </div>
      <div v-if="!settings" class="ez-skel mt-5 h-20"/>
      <div v-else class="mt-5 grid grid-cols-[repeat(auto-fit,minmax(160px,1fr))] gap-6 max-[560px]:grid-cols-2 max-[560px]:gap-3">
        <KpiTile label="Verbruik" color="bg-verbruik" :value="kpi?.t.verbruik ?? null" :yesterday="kpi?.y?.verbruik" :format="one" unit="kWh"/>
        <template v-if="hasSolar">
          <KpiTile label="Opname" color="bg-opname" :value="kpi?.t.opname ?? null" :yesterday="kpi?.y?.opname" :format="one" unit="kWh"/>
          <KpiTile label="Opwekking" color="bg-opwekking" :value="kpi?.t.opwekking ?? null" :yesterday="kpi?.y?.opwekking" :format="one" unit="kWh"/>
          <KpiTile label="Teruglevering" color="bg-teruglevering" :value="kpi?.t.teruglevering ?? null" :yesterday="kpi?.y?.teruglevering" :format="one" unit="kWh"/>
        </template>
        <KpiTile label="Gas" color="bg-gas" :value="kpi?.t.gas ?? null" :yesterday="kpi?.y?.gas" :format="two" unit="m³"/>
        <KpiTile label="Kosten" color="bg-ink-muted" :value="kpi?.t.kosten ?? null" :yesterday="kpi?.y?.kosten" :format="two" euro/>
      </div>
    </section>

    <div class="flex flex-wrap items-start gap-5 max-[560px]:gap-4">
      <div class="min-w-0 flex-[2_1_640px] max-md:basis-full">
        <VermogenChart v-if="settings" :time-zone="tz" :has-solar="hasSolar"/>
        <div v-else class="ez-card"><div class="ez-skel h-[380px]"/></div>
      </div>

      <!-- Meterstanden -->
      <section class="ez-card min-w-0 flex-[1_1_320px]" aria-labelledby="meter-title">
        <h2 id="meter-title" class="ez-title">Meterstanden</h2>
        <p class="ez-caption m-0">
          <template v-if="created">Stand om {{ time(created, tz, true) }}</template>
          <span v-else class="ez-skel inline-block h-4 w-28"/>
        </p>
        <dl class="mt-5 mb-0">
          <template v-if="reading">
            <div v-for="(m, i) in meters" :key="m.label" class="flex items-center justify-between gap-3 rounded-sm px-3 py-2"
                 :class="i % 2 === 0 ? 'bg-surface-sunken' : ''">
              <dt>{{ m.label }}</dt>
              <dd class="ez-meter m-0 text-right whitespace-nowrap">{{ m.text }} <span class="text-ink-muted">{{ m.unit }}</span></dd>
            </div>
          </template>
          <div v-else v-for="i in 4" :key="i" class="ez-skel mb-2 h-9"/>
        </dl>
        <p class="ez-caption mt-5 mb-0">Gas werkt de meter eens per uur bij.</p>
      </section>
    </div>
  </div>
</template>
