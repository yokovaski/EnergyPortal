import {describe, expect, it} from 'vitest';
import {allowedGroups, describe as describePeriod, presetPeriod} from './period';

const TZ = 'Europe/Amsterdam';
const now = new Date('2026-11-19T12:42:30Z');

describe('periods', () => {
    it('starts presets on local boundaries with a matching grouping', () => {
        expect(presetPeriod('7d', now, TZ)).toMatchObject({from: new Date('2026-11-12T23:00:00Z'), group: 'days'});
        expect(presetPeriod('jaar', now, TZ)).toMatchObject({from: new Date('2025-11-30T23:00:00Z'), group: 'months'});
        expect(presetPeriod('5jaar', now, TZ)).toMatchObject({from: new Date('2021-12-31T23:00:00Z'), group: 'years'});
    });
    it('describes the range and grouping', () => {
        expect(describePeriod(presetPeriod('7d', now, TZ), now, TZ)).toEqual({name: 'Laatste 7 dagen', detail: '13 – 19 nov · per dag'});
    });
    it('only offers groupings the server allows', () => {
        expect(allowedGroups(7)).toEqual(['hours', 'days', 'months', 'years']);
        expect(allowedGroups(31)).toEqual(['days', 'months', 'years']);
        expect(allowedGroups(800)).toEqual(['months', 'years']);
    });
});
