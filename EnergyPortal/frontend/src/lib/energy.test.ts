import {describe, expect, it} from 'vitest';
import {dayCost, flow, listNl, missingPrices} from './energy';
import {DEFAULT_SETTINGS} from '../composables/useSettings';

describe('flow', () => {
    it('stacks direct solar and opname up to verbruik', () => {
        const f = flow(0, 1.14, 0.5);
        expect(f.direct).toBeCloseTo(0.64);
        expect(f.verbruik).toBeCloseTo(0.64);
        expect(f.direct + f.opname).toBeCloseTo(f.verbruik);
    });
    it('never has negative direct solar (meter timing differences)', () => {
        expect(flow(0.3, 0.1, 0.2).direct).toBe(0);
    });
    it('ignores solar for households without panels', () => {
        expect(flow(0.4, 0.2, 0.1, false)).toEqual({verbruik: 0.4, opname: 0.4, opwekking: 0, teruglevering: 0, direct: 0});
    });
});

describe('costs', () => {
    const totals = {usageTotalHigh: 2, usageTotalLow: 1, redeliveryTotalHigh: 1, redeliveryTotalLow: 0, totalSolar: 3, totalGas: 2};
    const prices = {...DEFAULT_SETTINGS, highUsagePricePerKwh: 0.3, lowUsagePricePerKwh: 0.2, highRedeliveryPricePerKwh: 0.1, gasPrice: 1.5};
    it('adds opname and gas and subtracts teruglevering', () => {
        expect(dayCost(totals, prices)).toBeCloseTo(0.6 + 0.2 - 0.1 + 3);
    });
    it('charges for teruglevering with a negative feed-in price', () => {
        expect(dayCost(totals, {...prices, highRedeliveryPricePerKwh: -0.05})).toBeCloseTo(0.6 + 0.2 + 0.05 + 3);
    });
    it('names missing tariffs', () => {
        expect(missingPrices({...prices, gasPrice: 0})).toEqual(['de gasprijs']);
        expect(listNl(['a', 'b', 'c'])).toBe('a, b en c');
    });
});
