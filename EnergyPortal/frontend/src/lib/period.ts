// Historie periods: presets and custom ranges with a grouping the server accepts (spec §7.2).
import type {Group} from './api';
import {addDays, fromZoned, range, startOfDay, zoned} from './format';

export type Grouping = Extract<Group, 'hours' | 'days' | 'months' | 'years'>;
export type Preset = '24h' | '7d' | 'maand' | 'jaar' | '5jaar';

export interface Period {
    preset: Preset | 'custom';
    from: Date;
    /** Exclusive end; undefined = up to now. */
    to?: Date;
    group: Grouping;
}

export const PRESETS: { key: Preset; label: string }[] = [
    {key: '24h', label: 'Afgelopen 24 uur'},
    {key: '7d', label: 'Laatste 7 dagen'},
    {key: 'maand', label: 'Afgelopen maand'},
    {key: 'jaar', label: 'Afgelopen jaar'},
    {key: '5jaar', label: 'Afgelopen 5 jaar'},
];

export const GROUP_LABEL: Record<Grouping, string> = {hours: 'uur', days: 'dag', months: 'maand', years: 'jaar'};

const DAY = 24 * 3_600_000;
/** Longest range per grouping (MetricsWebApiController.TimeSpanTooLarge). */
export const MAX_DAYS: Record<Grouping, number> = {hours: 30, days: 700, months: Infinity, years: Infinity};

export function presetPeriod(preset: Preset, now: Date, tz: string): Period {
    const p = zoned(now, tz);
    switch (preset) {
        case '24h': return {preset, from: new Date(now.getTime() - DAY), group: 'hours'};
        case '7d': return {preset, from: startOfDay(addDays(now, -6, tz), tz), group: 'days'};
        case 'maand': return {preset, from: startOfDay(addDays(now, -29, tz), tz), group: 'days'};
        case 'jaar': return {preset, from: fromZoned(tz, p.year, p.month - 11, 1), group: 'months'};
        case '5jaar': return {preset, from: fromZoned(tz, p.year - 4, 1, 1), group: 'years'};
    }
}

/** Groupings allowed for a range (days inclusive). */
export function allowedGroups(days: number): Grouping[] {
    return (Object.keys(MAX_DAYS) as Grouping[]).filter(g => days <= MAX_DAYS[g]);
}

/** Days in a period, counting partial days. */
export function periodDays(p: Period, now: Date): number {
    return Math.ceil(((p.to ?? now).getTime() - p.from.getTime()) / DAY);
}

/** "Laatste 7 dagen" + "13 – 19 nov · per dag". */
export function describe(p: Period, now: Date, tz: string): { name: string; detail: string } {
    const name = p.preset === 'custom' ? 'Aangepast' : PRESETS.find(x => x.key === p.preset)!.label;
    const lastDay = new Date((p.to ?? now).getTime() - 1);
    return {name, detail: `${range(p.from, lastDay, tz)} · per ${GROUP_LABEL[p.group]}`};
}
