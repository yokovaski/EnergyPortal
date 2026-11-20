<script setup lang="ts">
import type {ApexOptions} from 'apexcharts';
import {computed, ref, watch} from 'vue';
import VueApexCharts from 'vue3-apexcharts';
import Banner from '../components/Banner.vue';
import ChartCard from '../components/ChartCard.vue';
import LegendTotal from '../components/LegendTotal.vue';
import SegmentedControl from '../components/SegmentedControl.vue';
import {useSettings} from '../composables/useSettings';
import {useMedia} from '../composables/useUi';
import {getSeries, type Series} from '../lib/api';
import {baseOptions, download, tooltipHtml, useChartColors, zeroLine} from '../lib/chart';
import {flow, listNl, missingPrices} from '../lib/energy';
import {day, euro, kwh, m3, monthLong, monthShort, num, range as rangeText, time, year, zoned} from '../lib/format';
import {GROUP_LABEL, type Period, presetPeriod} from '../lib/period';
import PeriodPicker from './historie/PeriodPicker.vue';

const {settings} = useSettings();
const tz = computed(() => settings.value?.timeZoneId ?? 'Europe/Amsterdam');
const hasSolar = computed(() => settings.value?.solarSystem ?? false);
const showDayName = computed(() => settings.value?.showDayName ?? true);
const mobile = useMedia('(max-width: 767px)');
const colors = useChartColors();

const period = ref<Period | null>(null);
const mode = ref<'kwh' | 'euro'>('kwh');
const tab = ref<'elektriciteit' | 'gas'>('elektriciteit');

watch(settings, s => {
  if (s && !period.value) period.value = presetPeriod('7d', new Date(), s.timeZoneId);
}, {immediate: true});

const data = ref<Series | null>(null);
const failed = ref(false);
watch(period, async p => {
  if (!p) return;
  data.value = null;
  failed.value = false;
  try {
    const result = await getSeries(p.group, p.from, p.to);
    if (p === period.value) data.value = result;
  } catch {
    failed.value = true;
  }
}, {immediate: true});

// ---- rows: one per bucket ----
interface Row {
  x: number; verbruik: number; opname: number; opwekking: number; teruglevering: number; direct: number;
  gas: number; opnameEuro: number; terugEuro: number; totaalEuro: number; gasEuro: number;
  temp: number | null; dd: number | null;
}
const rows = computed<Row[]>(() => {
  const s = data.value;
  if (!s) return [];
  return s.intake.map(([x, intake], i) => ({
    x, ...flow(intake, s.solar[i]?.[1] ?? 0, s.redelivery[i]?.[1] ?? 0, hasSolar.value),
    gas: s.gas[i]?.[1] ?? 0,
    opnameEuro: s.intakeCosts[i]?.[1] ?? 0,
    terugEuro: hasSolar.value ? (s.redeliveryCosts[i]?.[1] ?? 0) : 0,
    totaalEuro: s.usageCosts[i]?.[1] ?? 0,
    gasEuro: s.gasCosts[i]?.[1] ?? 0,
    temp: s.temperature?.[i]?.[1] ?? null,
    dd: s.degreeDays?.[i]?.[1] ?? null,
  }));
});
const total = computed(() => {
  const sum = (k: Exclude<keyof Row, 'temp' | 'dd'>) => rows.value.reduce((a, r) => a + r[k], 0);
  return {verbruik: sum('verbruik'), opname: sum('opname'), opwekking: sum('opwekking'), teruglevering: sum('teruglevering'),
    gas: sum('gas'), opnameEuro: sum('opnameEuro'), terugEuro: sum('terugEuro'), totaalEuro: sum('totaalEuro'), gasEuro: sum('gasEuro')};
});

const group = computed(() => period.value?.group ?? 'days');
// The server has no cost split per hour
const costsAvailable = computed(() => group.value !== 'hours');
const missing = computed(() => (settings.value ? missingPrices(settings.value) : []));
const gasPriceMissing = computed(() => missing.value.includes('de gasprijs'));

// ---- labels ----
function bucketLabel(x: number): string {
  const d = new Date(x);
  switch (group.value) {
    case 'hours': return time(d, tz.value);
    case 'days': return day(d, tz.value, showDayName.value);
    case 'months': return monthShort(d, tz.value);
    case 'years': return year(d, tz.value);
  }
}
function tipHeader(x: number): string {
  const d = new Date(x);
  switch (group.value) {
    case 'hours': return `${day(d, tz.value)}, ${time(d, tz.value)}–${time(new Date(x + 3_600_000), tz.value)}`;
    case 'days': return day(d, tz.value, showDayName.value);
    case 'months': return monthLong(d, tz.value);
    case 'years': return year(d, tz.value);
  }
}
/** Category labels, thinned on small screens (labels never rotate). */
const categories = computed(() => {
  const n = rows.value.length;
  const max = mobile.value ? 5 : 12;
  const every = Math.max(1, Math.ceil(n / max));
  return rows.value.map((r, i) => {
    if (i % every !== 0) return '';
    if (mobile.value && group.value === 'days') {
      const z = zoned(new Date(r.x), tz.value);
      return showDayName.value ? [['ma', 'di', 'wo', 'do', 'vr', 'za', 'zo'][z.weekday], String(z.day)] : String(z.day);
    }
    return bucketLabel(r.x);
  });
});

const rangeCaption = computed(() => {
  const p = period.value;
  if (!p) return '';
  return `per ${GROUP_LABEL[p.group]}, ${rangeText(p.from, new Date((p.to ?? new Date()).getTime() - 1), tz.value)}`;
});

const state = (hasData: boolean) => (!data.value ? (failed.value ? 'empty' : 'loading') : hasData ? 'data' : 'empty');
const elecHasData = computed(() => rows.value.some(r => r.verbruik || r.teruglevering));
const gasHasData = computed(() => rows.value.some(r => r.gas));

// ---- charts ----

const elecSeries = computed(() => {
  const r = rows.value;
  if (mode.value === 'euro') {
    return [
      {name: 'Opname €', data: r.map(d => d.opnameEuro)},
      ...(hasSolar.value ? [{name: 'Teruglevering €', data: r.map(d => d.terugEuro)}] : []),
    ];
  }
  // Verbruik as a thin cap on top of the stack: direct + opname = verbruik
  const top = Math.max(...r.map(d => d.direct + d.opname), 0.001);
  return [
    ...(hasSolar.value ? [{name: 'Opwekking', data: r.map(d => d.direct)}] : []),
    {name: 'Opname', data: r.map(d => d.opname)},
    {name: 'Verbruik', data: r.map(d => (d.verbruik > 0 ? top * 0.012 : 0))},
    ...(hasSolar.value ? [{name: 'Teruglevering', data: r.map(d => -d.teruglevering)}] : []),
  ];
});

function barOptions(colorsList: string[], yFmt: (v: number) => string, tip: (i: number) => string, negatives: boolean): ApexOptions {
  const c = colors.value;
  const base = baseOptions(c, yFmt);
  return {
    ...base,
    chart: {...base.chart, type: 'bar', stacked: true, height: mobile.value ? 240 : 280},
    colors: colorsList,
    plotOptions: {bar: {columnWidth: rows.value.length > 40 ? '85%' : '60%', borderRadius: 0}},
    fill: {opacity: 1},
    stroke: {width: 0},
    xaxis: {...base.xaxis, categories: categories.value, tickPlacement: 'between'},
    yaxis: {...base.yaxis, forceNiceScale: true},
    annotations: {yaxis: negatives ? [zeroLine(c)] : []},
    states: {hover: {filter: {type: 'none'}}, active: {filter: {type: 'none'}}},
    tooltip: {...base.tooltip, custom: ({dataPointIndex}: { dataPointIndex: number }) => tip(dataPointIndex)},
  };
}

const elecOptions = computed<ApexOptions>(() => {
  const c = colors.value;
  const euroMode = mode.value === 'euro';
  const list = euroMode
      ? [c.opname, ...(hasSolar.value ? [c.teruglevering] : [])]
      : [...(hasSolar.value ? [c.opwekking] : []), c.opname, c.verbruik, ...(hasSolar.value ? [c.teruglevering] : [])];
  return barOptions(list, v => (euroMode ? euro(v, 0) : num(v, 0)), i => {
    const r = rows.value[i];
    if (!r) return '';
    return tooltipHtml(tipHeader(r.x), euroMode ? [
      {label: 'Totaal', color: c.ink, value: euro(r.totaalEuro), line: true},
      {label: 'Opname', color: c.opname, value: euro(r.opnameEuro)},
      ...(hasSolar.value ? [{label: 'Teruglevering', color: c.teruglevering, value: euro(r.terugEuro)}] : []),
    ] : [
      {label: 'Verbruik', color: c.verbruik, value: kwh(r.verbruik), line: true},
      {label: 'Opname', color: c.opname, value: kwh(r.opname)},
      ...(hasSolar.value ? [
        {label: 'Opwekking', color: c.opwekking, value: kwh(r.opwekking)},
        {label: 'Teruglevering', color: c.teruglevering, value: kwh(r.teruglevering)},
      ] : []),
    ]);
  }, hasSolar.value);
});

// ---- outdoor temperature (KNMI), shown with gas ----
const hasTemp = computed(() => rows.value.some(r => r.temp != null));
const showTemp = computed(() => hasTemp.value && mode.value === 'kwh');
const temp = computed(() => {
  const t = rows.value.filter(r => r.temp != null);
  const withDd = rows.value.filter(r => r.dd != null);
  const dd = withDd.reduce((a, r) => a + r.dd!, 0);
  return {
    mean: t.length ? t.reduce((a, r) => a + r.temp!, 0) / t.length : null,
    // Gas per heating degree day: the number that drops when insulation works, independent of how cold it was
    perDd: dd >= 1 ? withDd.reduce((a, r) => a + r.gas, 0) / dd : null,
  };
});
const celsius = (v: number | null) => (v == null ? '–' : `${num(v, 1)} °C`);

const gasSeries = computed(() => [
  {name: 'Gas', type: 'bar', data: rows.value.map(r => (mode.value === 'euro' ? r.gasEuro : r.gas))},
  ...(showTemp.value ? [{name: 'Buitentemperatuur', type: 'line', data: rows.value.map(r => r.temp)}] : []),
]);
const gasOptions = computed<ApexOptions>(() => {
  const c = colors.value;
  const euroMode = mode.value === 'euro';
  const options = barOptions([c.gas, c.ink], v => (euroMode ? euro(v, 0) : num(v, 1)), i => {
    const r = rows.value[i];
    return r ? tooltipHtml(tipHeader(r.x), [
      {label: 'Gas', color: c.gas, value: euroMode ? euro(r.gasEuro) : m3(r.gas)},
      ...(showTemp.value ? [{label: 'Buiten', color: c.ink, value: celsius(r.temp), line: true},
        ...(r.dd != null && group.value !== 'hours' ? [{label: 'Graaddagen', color: 'transparent', value: num(r.dd, 1)}] : [])] : []),
    ]) : '';
  }, false);
  if (!showTemp.value) return options;
  const labelStyle = (options.yaxis as ApexYAxis).labels!.style;
  return {
    ...options,
    chart: {...options.chart, type: 'line'},
    stroke: {width: [0, 2], curve: 'smooth'},
    markers: {size: 0},
    yaxis: [options.yaxis as ApexYAxis,
      {opposite: true, forceNiceScale: true, labels: {style: labelStyle, formatter: v => `${num(v, 0)}°`}}],
  };
});

const gasTempNote = computed(() => {
  if (!settings.value?.postcode) return ' Stel je postcode in onder Instellingen om de buitentemperatuur te zien.';
  if (showTemp.value) return ` Buitentemperatuur van KNMI-station ${settings.value.weatherStation ?? ''}; graaddagen met basis 18 °C.`;
  return '';
});

const elecFootnote = computed(() => mode.value === 'euro'
    ? 'Bedragen inclusief vaste kosten. Teruglevering staat onder de nul.'
    : hasSolar.value ? 'Teruglevering staat onder de nul. De laatste periode is nog niet compleet.' : 'De laatste periode is nog niet compleet.');

function perDay() {
  if (period.value) period.value = {...period.value, group: 'days'};
}

// ---- table + CSV ----
const tableTitle = computed(() => `Per ${GROUP_LABEL[group.value]}`);
const firstCol = computed(() => GROUP_LABEL[group.value][0].toUpperCase() + GROUP_LABEL[group.value].slice(1));
const rowCost = (r: Row) => r.totaalEuro + r.gasEuro;

function exportCsv() {
  const n = (v: number, d: number) => v.toFixed(d).replace('.', ',');
  const head = [firstCol.value, 'Verbruik (kWh)', 'Opname (kWh)', ...(hasSolar.value ? ['Opwekking (kWh)', 'Teruglevering (kWh)'] : []),
    'Gas (m³)', ...(hasTemp.value ? ['Buiten (°C)', 'Graaddagen'] : []), ...(costsAvailable.value ? ['Kosten (€)'] : [])];
  const lines = rows.value.map(r => [tipHeader(r.x), n(r.verbruik, 3), n(r.opname, 3),
    ...(hasSolar.value ? [n(r.opwekking, 3), n(r.teruglevering, 3)] : []), n(r.gas, 3),
    ...(hasTemp.value ? [r.temp == null ? '' : n(r.temp, 1), r.dd == null ? '' : n(r.dd, 2)] : []), ...(costsAvailable.value ? [n(rowCost(r), 2)] : [])]);
  const p = period.value!;
  const name = `energiezicht-${p.from.toISOString().slice(0, 10)}-per-${GROUP_LABEL[p.group]}.csv`;
  download(name, [head, ...lines].map(l => l.join(';')).join('\r\n'));
}
</script>

<template>
  <div class="flex flex-col gap-5 max-[560px]:gap-4">
    <!-- Toolbar -->
    <div class="flex flex-wrap items-center gap-x-6 gap-y-3">
      <h1 class="ez-title-lg">Historie</h1>
      <PeriodPicker v-if="period" v-model="period" :time-zone="tz" class="min-w-0 max-[560px]:order-3 max-[560px]:w-full"/>
      <span v-else class="ez-skel h-10 w-72"/>
      <SegmentedControl v-model="mode" class="ml-auto" label="Eenheid"
                        :options="[{ value: 'kwh', label: 'kWh' }, { value: 'euro', label: '€' }]"/>
    </div>

    <Banner v-if="missing.length" variant="warning">
      <strong>Niet alle prijzen zijn bekend.</strong> Stel {{ listNl(missing) }} in onder <RouterLink to="/settings" class="ez-link">Instellingen</RouterLink>.
    </Banner>

    <!-- Mobile tabs -->
    <div v-if="mobile" role="tablist" aria-label="Grafiek" class="flex border-b border-line">
      <button v-for="t in (['elektriciteit', 'gas'] as const)" :key="t" role="tab" type="button" :aria-selected="tab === t"
              class="h-11 flex-1 border-b-2 font-medium" :class="tab === t ? 'border-brand text-brand font-semibold' : 'border-transparent text-ink-muted'"
              @click="tab = t">{{ t === 'gas' ? 'Gas' : 'Elektriciteit' }}</button>
    </div>

    <div class="flex flex-wrap gap-5 max-[560px]:gap-4">
      <div v-show="!mobile || tab === 'elektriciteit'" class="min-w-0 flex-[1_1_480px]">
        <ChartCard title="Elektriciteit" :caption="`${mode === 'euro' ? 'Kosten' : 'Energie'} ${rangeCaption}`"
                   :state="mode === 'euro' && !costsAvailable ? 'empty' : state(elecHasData)" :unit="mode === 'euro' ? '€' : 'kWh'" :height="mobile ? 240 : 280" :footnote="elecFootnote">
          <template #legend>
            <template v-if="mode === 'euro'">
              <LegendTotal label="Totaal" color="bg-ink" line euro :value="data && costsAvailable ? num(total.totaalEuro, 2) : null"/>
              <LegendTotal label="Opname" color="bg-opname" euro :value="data && costsAvailable ? num(total.opnameEuro, 2) : null"/>
              <LegendTotal v-if="hasSolar" label="Teruglevering" color="bg-teruglevering" euro :value="data && costsAvailable ? num(total.terugEuro, 2) : null"/>
            </template>
            <template v-else>
              <LegendTotal label="Verbruik" color="bg-verbruik" line unit="kWh" :value="data ? num(total.verbruik, 1) : null"/>
              <LegendTotal label="Opname" color="bg-opname" unit="kWh" :value="data ? num(total.opname, 1) : null"/>
              <template v-if="hasSolar">
                <LegendTotal label="Opwekking" color="bg-opwekking" unit="kWh" :value="data ? num(total.opwekking, 1) : null"/>
                <LegendTotal label="Teruglevering" color="bg-teruglevering" unit="kWh" :value="data ? num(total.teruglevering, 1) : null"/>
              </template>
            </template>
          </template>
          <template v-if="mode === 'euro' && !costsAvailable" #empty>
            <p class="m-0 text-ink-muted">Kosten zijn er per dag, maand of jaar.</p>
            <button type="button" class="ez-btn" @click="perDay">Per dag bekijken</button>
          </template>
          <div role="img" :aria-label="`Elektriciteit ${rangeCaption}. Details staan in de tabel hieronder.`">
            <VueApexCharts type="bar" :height="mobile ? 240 : 280" :options="elecOptions" :series="elecSeries"/>
          </div>
        </ChartCard>
      </div>

      <div v-show="!mobile || tab === 'gas'" class="min-w-0 flex-[1_1_480px]">
        <ChartCard title="Gas" :caption="`${mode === 'euro' ? 'Kosten' : 'Verbruik'} ${rangeCaption}`"
                   :state="mode === 'euro' && (gasPriceMissing || !costsAvailable) ? 'empty' : state(gasHasData)" :unit="mode === 'euro' ? '€' : 'm³'" :height="mobile ? 240 : 280"
:footnote="`De gasmeter werkt eens per uur bij. De laatste periode is nog niet compleet.${gasTempNote}`">
          <template #legend>
            <LegendTotal v-if="mode === 'euro'" label="Gas" color="bg-gas" euro
                         :value="data && costsAvailable && !gasPriceMissing ? num(total.gasEuro, 2) : null"/>
            <LegendTotal v-else label="Gas" color="bg-gas" unit="m³" :value="data ? num(total.gas, 2) : null"/>
            <template v-if="showTemp">
              <LegendTotal label="Gem. buiten" color="bg-ink" line unit="°C" :value="temp.mean == null ? '–' : num(temp.mean, 1)"/>
              <LegendTotal v-if="temp.perDd != null" label="Per graaddag" color="bg-transparent" unit="m³" :value="num(temp.perDd, 2)"/>
            </template>
          </template>
          <template v-if="mode === 'euro' && gasPriceMissing" #empty>
            <p class="m-0 text-ink-muted">Zonder gasprijs kunnen we geen gaskosten laten zien.</p>
            <RouterLink to="/settings#tarieven" class="ez-btn">Naar Instellingen</RouterLink>
          </template>
          <template v-else-if="mode === 'euro' && !costsAvailable" #empty>
            <p class="m-0 text-ink-muted">Kosten zijn er per dag, maand of jaar.</p>
            <button type="button" class="ez-btn" @click="perDay">Per dag bekijken</button>
          </template>
          <div role="img" :aria-label="`Gas ${rangeCaption}. Details staan in de tabel hieronder.`">
            <VueApexCharts :type="showTemp ? 'line' : 'bar'" :height="mobile ? 240 : 280" :options="gasOptions" :series="gasSeries"/>
          </div>
        </ChartCard>
      </div>
    </div>

    <!-- Per periode -->
    <section class="ez-card" aria-labelledby="table-title">
      <div class="flex flex-wrap items-center justify-between gap-3">
        <h2 id="table-title" class="ez-title">{{ tableTitle }}</h2>
        <button type="button" class="ez-btn" :disabled="!rows.length" @click="exportCsv">Exporteren</button>
      </div>
      <div v-if="!data" class="mt-4 flex flex-col gap-2"><div v-for="i in 6" :key="i" class="ez-skel h-9"/></div>
      <div v-else class="mt-4 overflow-x-auto">
        <table class="w-full border-collapse text-right tabular-nums">
          <thead>
            <tr class="border-b border-line-strong align-bottom">
              <th scope="col" class="ez-label px-3 py-2 text-left">{{ firstCol }}</th>
              <th scope="col" class="ez-label px-3 py-2">Verbruik<span class="ez-caption block">kWh</span></th>
              <template v-if="!mobile">
                <th scope="col" class="ez-label px-3 py-2">Opname<span class="ez-caption block">kWh</span></th>
                <template v-if="hasSolar">
                  <th scope="col" class="ez-label px-3 py-2">Opwekking<span class="ez-caption block">kWh</span></th>
                  <th scope="col" class="ez-label px-3 py-2">Teruglevering<span class="ez-caption block">kWh</span></th>
                </template>
              </template>
              <th scope="col" class="ez-label px-3 py-2">Gas<span class="ez-caption block">m³</span></th>
              <th v-if="hasTemp" scope="col" class="ez-label px-3 py-2">Buiten<span class="ez-caption block">°C</span></th>
              <th v-if="costsAvailable" scope="col" class="ez-label px-3 py-2">Kosten<span class="ez-caption block">{{ gasPriceMissing ? '€, zonder gas' : '€' }}</span></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in rows" :key="r.x" class="border-b border-line">
              <th scope="row" class="px-3 py-2 text-left font-normal whitespace-nowrap">{{ group === 'hours' ? tipHeader(r.x) : bucketLabel(r.x) }}</th>
              <td class="px-3 py-2">{{ num(r.verbruik, 1) }}</td>
              <template v-if="!mobile">
                <td class="px-3 py-2">{{ num(r.opname, 1) }}</td>
                <template v-if="hasSolar">
                  <td class="px-3 py-2">{{ num(r.opwekking, 1) }}</td>
                  <td class="px-3 py-2">{{ num(r.teruglevering, 1) }}</td>
                </template>
              </template>
              <td class="px-3 py-2">{{ num(r.gas, 2) }}</td>
              <td v-if="hasTemp" class="px-3 py-2">{{ r.temp == null ? '–' : num(r.temp, 1) }}</td>
              <td v-if="costsAvailable" class="px-3 py-2 whitespace-nowrap">{{ euro(rowCost(r)) }}</td>
            </tr>
          </tbody>
          <tfoot>
            <tr class="bg-surface-sunken font-semibold">
              <th scope="row" class="px-3 py-2 text-left">Totaal</th>
              <td class="px-3 py-2">{{ num(total.verbruik, 1) }}</td>
              <template v-if="!mobile">
                <td class="px-3 py-2">{{ num(total.opname, 1) }}</td>
                <template v-if="hasSolar">
                  <td class="px-3 py-2">{{ num(total.opwekking, 1) }}</td>
                  <td class="px-3 py-2">{{ num(total.teruglevering, 1) }}</td>
                </template>
              </template>
              <td class="px-3 py-2">{{ num(total.gas, 2) }}</td>
              <td v-if="hasTemp" class="px-3 py-2">{{ temp.mean == null ? '–' : num(temp.mean, 1) }}</td>
              <td v-if="costsAvailable" class="px-3 py-2 whitespace-nowrap">{{ euro(total.totaalEuro + total.gasEuro) }}</td>
            </tr>
          </tfoot>
        </table>
      </div>
    </section>
  </div>
</template>
