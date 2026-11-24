import {describe, expect, it} from 'vitest';
import {addDays, day, dmy, euro, fromZoned, kw, kwh, m3, meter, monthLong, monthShort, num, parseAmount, parseDmy, range, startOfDay, tariff, time, year} from './format';

const TZ = 'Europe/Amsterdam';
// do 19 nov 2026 13:42:30 in Amsterdam (UTC+1)
const now = new Date('2026-11-19T12:42:30Z');

describe('numbers', () => {
    it('formats the spec examples', () => {
        expect(kw(0.64)).toBe('0,640 kW');
        expect(kwh(68.25)).toBe('68,3 kWh');
        expect(m3(3.42)).toBe('3,42 m³');
        expect(euro(1.87)).toBe('€ 1,87');
        expect(euro(-1)).toBe('−€ 1,00');
        expect(meter(4277.964)).toBe('4.277,964');
        expect(tariff(0.272)).toBe('€ 0,2720');
    });
    it('never shows a negative zero', () => {
        expect(num(-0.0001, 3)).toBe('0,000');
        expect(euro(-0.001)).toBe('€ 0,00');
    });
    it('parses both decimal separators', () => {
        expect(parseAmount('0,272')).toBe(0.272);
        expect(parseAmount('0.272')).toBe(0.272);
        expect(parseAmount('€ 1.234,50')).toBe(1234.5);
        expect(parseAmount('-0,05')).toBe(-0.05);
        expect(parseAmount('abc')).toBeNull();
        expect(parseAmount('')).toBeNull();
    });
});

describe('dates', () => {
    it('formats times zero-padded in the user time zone', () => {
        expect(time(now, TZ)).toBe('13:42');
        expect(time(now, TZ, true)).toBe('13:42:30');
        expect(time(new Date('2026-11-19T08:05:03Z'), TZ, true)).toBe('09:05:03');
    });
    it('formats days and months in Dutch without dots', () => {
        expect(day(now, TZ)).toBe('do 19 nov');
        expect(day(new Date('2026-11-13T12:00:00Z'), TZ)).toBe('vr 13 nov');
        expect(day(now, TZ, false)).toBe('19 nov');
        expect(monthShort(new Date('2026-03-10T12:00:00Z'), TZ)).toBe('mrt');
        expect(monthLong(now, TZ)).toBe('november 2026');
        expect(year(now, TZ)).toBe('2026');
    });
    it('uses the local day, not the UTC day, near midnight', () => {
        // 23:30 UTC on the 19th is already the 20th in Amsterdam
        expect(day(new Date('2026-11-19T23:30:00Z'), TZ)).toBe('vr 20 nov');
    });
    it('formats ranges', () => {
        expect(range(new Date('2026-11-13T12:00:00Z'), now, TZ)).toBe('13 – 19 nov');
        expect(range(new Date('2026-10-28T12:00:00Z'), new Date('2026-11-03T12:00:00Z'), TZ)).toBe('28 okt – 3 nov');
        expect(range(new Date('2025-12-28T12:00:00Z'), new Date('2026-01-03T12:00:00Z'), TZ)).toBe('28 dec 2025 – 3 jan 2026');
    });
    it('finds local midnight across DST', () => {
        expect(startOfDay(now, TZ).toISOString()).toBe('2026-11-18T23:00:00.000Z');
        expect(fromZoned(TZ, 2026, 7, 1).toISOString()).toBe('2026-06-30T22:00:00.000Z');
        // the night of 25 oct 2026 has 25 hours
        expect(addDays(fromZoned(TZ, 2026, 10, 25), 1, TZ).toISOString()).toBe('2026-10-25T23:00:00.000Z');
    });
    it('round-trips dd-mm-jjjj', () => {
        expect(dmy(now, TZ)).toBe('19-11-2026');
        expect(parseDmy('19-11-2026', TZ)?.toISOString()).toBe('2026-11-18T23:00:00.000Z');
        expect(parseDmy('31-02-2026', TZ)).toBeNull();
        expect(parseDmy('nope', TZ)).toBeNull();
    });
});
