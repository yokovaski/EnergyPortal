<script setup lang="ts">
import type {ApexOptions} from 'apexcharts';
import {computed, onUnmounted, ref, watch} from 'vue';
import VueApexCharts from 'vue3-apexcharts';
import ChartCard from '../../components/ChartCard.vue';
import LegendTotal from '../../components/LegendTotal.vue';
import SegmentedControl from '../../components/SegmentedControl.vue';
import {useMedia} from '../../composables/useUi';
import {getSeries, type Group, type Series} from '../../lib/api';
import {baseOptions, tooltipHtml, useChartColors, zeroLine} from '../../lib/chart';
import {flow} from '../../lib/energy';
import {day, fromZoned, kw, num, startOfDay, time, zoned} from '../../lib/format';

const props = defineProps<{ timeZone: string; hasSolar: boolean }>();

type Range = 'nu' | 'uur' | 'dag' | 'week';
const range = ref<Range>('dag');
const options = [
  {value: 'nu' as const, label: 'Nu'}, {value: 'uur' as const, label: 'Uur'},
  {value: 'dag' as const, label: 'Dag'}, {value: 'week' as const, label: 'Week'},
];
const mobile = useMedia('(max-width: 767px)');
const colors = useChartColors();

const MIN = 60_000, HOUR = 3_600_000;
// group, hours per point (to turn kW into kWh), refresh interval, caption
const config: Record<Range, { group: Group; hoursPerPoint: number; refresh: number; caption: string }> = {
  nu: {group: 'tenseconds', hoursPerPoint: 10 / 3600, refresh: 10_000, caption: 'Afgelopen 10 minuten, per 10 seconden'},
  uur: {group: 'minutes', hoursPerPoint: 1 / 60, refresh: MIN, caption: 'Afgelopen uur, gemiddeld per minuut'},
  dag: {group: 'hours', hoursPerPoint: 1, refresh: 5 * MIN, caption: 'Afgelopen 24 uur, gemiddeld per uur'},
  week: {group: 'hours', hoursPerPoint: 1, refresh: 5 * MIN, caption: 'Afgelopen 7 dagen, gemiddeld per uur'},
};

/** Axis window ending at the next whole tick in the user's time zone: [min, max] and tick step. */
function axisWindow(r: Range, now: Date, tz: string) {
  const ceilTo = (stepMin: number) => {
    const p = zoned(now, tz);
    const minutes = Math.ceil((p.hour * 60 + p.minute + p.second / 60) / stepMin) * stepMin;
    return fromZoned(tz, p.year, p.month, p.day, 0, minutes).getTime();
  };
  switch (r) {
    case 'nu': { const max = ceilTo(2); return {min: max - 10 * MIN, max, step: 2 * MIN}; }
    case 'uur': { const max = ceilTo(10); return {min: max - HOUR, max, step: 10 * MIN}; }
    case 'dag': {
      const step = mobile.value ? 6 : 2;
      const max = ceilTo(step * 60);
      return {min: max - 24 * HOUR, max, step: step * HOUR};
    }
    case 'week': {
      const max = startOfDay(new Date(now.getTime() + 24 * HOUR), tz).getTime();
      return {min: max - 7 * 24 * HOUR, max, step: 24 * HOUR};
    }
  }
}

const data = ref<Series | null>(null);
const failed = ref(false);
const loadedAt = ref(new Date());
let timer: number | undefined;

async function load() {
  const r = range.value;
  const now = new Date();
  const back = {nu: 10 * MIN, uur: HOUR, dag: 24 * HOUR, week: 7 * 24 * HOUR}[r];
  try {
    const result = await getSeries(config[r].group, new Date(now.getTime() - back));
    if (r !== range.value) return; // range changed while loading
    data.value = result;
    failed.value = false;
    loadedAt.value = now;
  } catch {
    failed.value = true;
  }
}

watch(range, () => {
  data.value = null;
  clearInterval(timer);
  void load();
  timer = window.setInterval(load, config[range.value].refresh);
}, {immediate: true});
onUnmounted(() => clearInterval(timer));

/** One point per timestamp with the derived flows (kW). */
const points = computed(() => {
  const s = data.value;
  if (!s) return [];
  return s.intake.map(([x, intake], i) => ({x, ...flow(intake, s.solar[i]?.[1] ?? 0, s.redelivery[i]?.[1] ?? 0, props.hasSolar)}));
});

const state = computed(() => {
  if (!data.value) return failed.value ? 'empty' : 'loading';
  return points.value.some(p => p.verbruik !== 0 || p.teruglevering !== 0) ? 'data' : 'empty';
});

const totals = computed(() => {
  const h = config[range.value].hoursPerPoint;
  const sum = (k: 'verbruik' | 'opname' | 'opwekking' | 'teruglevering') => points.value.reduce((a, p) => a + p[k] * h, 0);
  return {verbruik: sum('verbruik'), opname: sum('opname'), opwekking: sum('opwekking'), teruglevering: sum('teruglevering')};
});

const series = computed(() => {
  const p = points.value;
  const list: ApexAxisChartSeries = [];
  if (props.hasSolar) list.push({name: 'Opwekking', type: 'rangeArea', data: p.map(d => ({x: d.x, y: [0, d.direct]}))});
  list.push({name: 'Opname', type: 'rangeArea', data: p.map(d => ({x: d.x, y: [d.direct, d.direct + d.opname]}))});
  if (props.hasSolar) list.push({name: 'Teruglevering', type: 'rangeArea', data: p.map(d => ({x: d.x, y: [-d.teruglevering, 0]}))});
  list.push({name: 'Verbruik', type: 'line', data: p.map(d => ({x: d.x, y: d.verbruik}))});
  return list;
});

function tipHeader(x: number): string {
  const d = new Date(x), tz = props.timeZone;
  switch (range.value) {
    case 'nu': return `${day(d, tz)}, ${time(d, tz, true)}`;
    case 'uur': return `${day(d, tz)}, ${time(d, tz)}`;
    default: return `${day(d, tz)}, ${time(d, tz)}–${time(new Date(x + HOUR), tz)}`;
  }
}

const chartOptions = computed<ApexOptions>(() => {
  const c = colors.value, tz = props.timeZone;
  const win = axisWindow(range.value, loadedAt.value, tz);
  const base = baseOptions(c, v => num(v, 1));
  const seriesColors = [...(props.hasSolar ? [c.opwekking] : []), c.opname, ...(props.hasSolar ? [c.teruglevering] : []), c.verbruik];
  const opacities = [...(props.hasSolar ? [0.45] : []), 0.40, ...(props.hasSolar ? [0.45] : []), 1];
  // Midnight markers with the day name (Dag and Week)
  const midnights: XAxisAnnotations[] = [];
  if (range.value === 'dag') {
    for (let t = startOfDay(new Date(win.max), tz).getTime(); t > win.min; t -= 24 * HOUR) {
      midnights.push({x: t, borderColor: c.lineStrong, strokeDashArray: 4,
        label: {text: day(new Date(t), tz), borderWidth: 0, orientation: 'horizontal', position: 'bottom', offsetY: 46,
          style: {background: 'transparent', color: c.inkMuted, fontSize: '12px'}}});
    }
  }
  const tickLabel = (v: number) => {
    const d = new Date(v);
    if (range.value === 'week') return mobile.value ? `${['ma', 'di', 'wo', 'do', 'vr', 'za', 'zo'][zoned(d, tz).weekday]} ${zoned(d, tz).day}` : day(d, tz);
    return time(d, tz);
  };
  return {
    ...base,
    chart: {...base.chart, type: 'rangeArea', height: mobile.value ? 240 : 300},
    colors: seriesColors,
    fill: {opacity: opacities, type: 'solid'},
    stroke: {width: [...seriesColors.slice(0, -1).map(() => 0), 2.5], curve: 'straight'},
    markers: {size: 0, hover: {size: 4}},
    xaxis: {
      ...base.xaxis, type: 'numeric', min: win.min, max: win.max, tickAmount: Math.round((win.max - win.min) / win.step),
      labels: {...base.xaxis!.labels, formatter: v => tickLabel(Number(v)), offsetY: 0},
    },
    yaxis: {...base.yaxis, forceNiceScale: true},
    grid: {...base.grid, padding: {...base.grid!.padding, bottom: range.value === 'dag' ? 20 : 0}},
    annotations: {yaxis: props.hasSolar ? [zeroLine(c)] : [], xaxis: midnights},
    tooltip: {
      ...base.tooltip,
      custom: ({dataPointIndex}: { dataPointIndex: number }) => {
        const p = points.value[dataPointIndex];
        if (!p) return '';
        return tooltipHtml(tipHeader(p.x), [
          {label: 'Verbruik', color: c.verbruik, value: kw(p.verbruik), line: true},
          {label: 'Opname', color: c.opname, value: kw(p.opname)},
          ...(props.hasSolar ? [
            {label: 'Opwekking', color: c.opwekking, value: kw(p.opwekking)},
            {label: 'Teruglevering', color: c.teruglevering, value: kw(p.teruglevering)},
          ] : []),
        ]);
      },
    },
  };
});
</script>

<template>
  <ChartCard title="Vermogen" :caption="config[range].caption" :state="state" unit="kW" :height="mobile ? 240 : 300"
             :footnote="hasSolar ? 'Teruglevering staat onder de nul.' : undefined">
    <template #actions>
      <SegmentedControl v-model="range" :options="options" label="Periode van de grafiek"/>
    </template>
    <template #legend>
      <LegendTotal label="Verbruik" color="bg-verbruik" line :value="data ? num(totals.verbruik, 1) : null" unit="kWh"/>
      <LegendTotal label="Opname" color="bg-opname" :value="data ? num(totals.opname, 1) : null" unit="kWh"/>
      <template v-if="hasSolar">
        <LegendTotal label="Opwekking" color="bg-opwekking" :value="data ? num(totals.opwekking, 1) : null" unit="kWh"/>
        <LegendTotal label="Teruglevering" color="bg-teruglevering" :value="data ? num(totals.teruglevering, 1) : null" unit="kWh"/>
      </template>
    </template>
    <div role="img" :aria-label="`Vermogen, ${config[range].caption.toLowerCase()}. Totaal verbruik ${num(totals.verbruik, 1)} kWh.`">
      <VueApexCharts type="rangeArea" :height="mobile ? 240 : 300" :options="chartOptions" :series="series"/>
    </div>
  </ChartCard>
</template>
