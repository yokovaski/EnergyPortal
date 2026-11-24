// Shared ApexCharts configuration (spec §6). Colours are read from the design tokens, so charts follow the theme.
import type {ApexOptions} from 'apexcharts';
import {computed, ref} from 'vue';

const themeVersion = ref(0);
new MutationObserver(() => themeVersion.value++)
    .observe(document.documentElement, {attributes: true, attributeFilter: ['data-theme']});

const token = (name: string) => getComputedStyle(document.documentElement).getPropertyValue(`--ez-${name}`).trim();

export type SeriesKey = 'verbruik' | 'opname' | 'opwekking' | 'teruglevering' | 'gas';

/** Token colours as plain values (SVG attributes can't use CSS variables). Recomputed on theme change. */
export function useChartColors() {
    return computed(() => {
        void themeVersion.value;
        return {
            line: token('line'), lineStrong: token('line-strong'), ink: token('ink'), inkMuted: token('ink-muted'),
            sunken: token('surface-sunken'),
            verbruik: token('series-verbruik'), opname: token('series-opname'), opwekking: token('series-opwekking'),
            teruglevering: token('series-teruglevering'), gas: token('series-gas'),
        };
    });
}

export type ChartColors = ReturnType<typeof useChartColors>['value'];

export function baseOptions(c: ChartColors, yFormatter: (v: number) => string): ApexOptions {
    const labelStyle = {colors: c.inkMuted, fontSize: '12px', fontFamily: 'inherit'};
    return {
        chart: {
            toolbar: {show: false}, zoom: {enabled: false}, selection: {enabled: false}, animations: {enabled: false},
            fontFamily: 'inherit', foreColor: c.inkMuted, background: 'transparent', parentHeightOffset: 0,
        },
        legend: {show: false},
        dataLabels: {enabled: false},
        grid: {borderColor: c.line, padding: {left: 4, right: 8}},
        states: {hover: {filter: {type: 'none'}}, active: {filter: {type: 'none'}}},
        xaxis: {
            labels: {rotate: 0, hideOverlappingLabels: true, style: labelStyle},
            axisBorder: {color: c.line}, axisTicks: {color: c.line}, tooltip: {enabled: false},
        },
        yaxis: {labels: {style: labelStyle, formatter: yFormatter}},
        tooltip: {shared: true, intersect: false},
    };
}

/** Zero line in line-strong, used whenever a series goes negative. */
export const zeroLine = (c: ChartColors) => ({y: 0, borderColor: c.lineStrong, strokeDashArray: 0, opacity: 1});

export interface TipRow { label: string; color: string; value: string; line?: boolean }

const esc = (s: string) => s.replace(/[&<>"]/g, ch => ({'&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;'}[ch]!));

/** Tooltip HTML in Card style (classes in app.css). */
export function tooltipHtml(header: string, rows: TipRow[]): string {
    return `<div class="ez-tip"><div class="ez-tip-head">${esc(header)}</div>${rows.map(r =>
        `<div class="ez-tip-row"><span class="${r.line ? 'ez-swatch-line' : 'ez-swatch'}" style="background:${r.color}"></span>` +
        `<span>${esc(r.label)}</span><span class="ez-tip-val">${esc(r.value)}</span></div>`).join('')}</div>`;
}

/** Download text as a file. */
export function download(filename: string, text: string, type = 'text/csv;charset=utf-8') {
    const url = URL.createObjectURL(new Blob(['﻿' + text], {type}));
    const a = Object.assign(document.createElement('a'), {href: url, download: filename});
    a.click();
    URL.revokeObjectURL(url);
}
