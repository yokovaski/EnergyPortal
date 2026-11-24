// Derived energy values (spec §5): verbruik = opname + direct, direct = opwekking − teruglevering.
import type {DayTotals, Settings} from './api';

export interface Flow {
    verbruik: number;
    opname: number;
    opwekking: number;
    teruglevering: number;
    /** Solar used in the house: the bottom of the stack. */
    direct: number;
}

export function flow(intake: number, solar: number, redelivery: number, hasSolar = true): Flow {
    if (!hasSolar) return {verbruik: intake, opname: intake, opwekking: 0, teruglevering: 0, direct: 0};
    const direct = Math.max(0, solar - redelivery);
    return {verbruik: intake + direct, opname: intake, opwekking: solar, teruglevering: redelivery, direct};
}

export function todayFlow(t: DayTotals, hasSolar: boolean): Flow & { gas: number } {
    return {
        ...flow(t.usageTotalHigh + t.usageTotalLow, t.totalSolar, t.redeliveryTotalHigh + t.redeliveryTotalLow, hasSolar),
        gas: t.totalGas,
    };
}

/** Electricity + gas cost without vaste kosten. A negative feed-in price makes teruglevering cost money. */
export function dayCost(t: DayTotals, s: Settings): number {
    return t.usageTotalHigh * s.highUsagePricePerKwh + t.usageTotalLow * s.lowUsagePricePerKwh
        - (t.redeliveryTotalHigh * s.highRedeliveryPricePerKwh + t.redeliveryTotalLow * s.lowRedeliveryPricePerKwh)
        + t.totalGas * s.gasPrice;
}

/** Names of tariffs that are not filled in (0), in Dutch, for the "Niet alle prijzen zijn bekend" banner. */
export function missingPrices(s: Settings): string[] {
    const missing: string[] = [];
    if (!s.highUsagePricePerKwh || !s.lowUsagePricePerKwh) missing.push('de stroomprijs');
    if (s.solarSystem && !s.highRedeliveryPricePerKwh && !s.lowRedeliveryPricePerKwh) missing.push('de terugleververgoeding');
    if (!s.gasPrice) missing.push('de gasprijs');
    return missing;
}

/** "a", "a en b", "a, b en c". */
export function listNl(items: string[]): string {
    return items.length <= 1 ? items.join('') : `${items.slice(0, -1).join(', ')} en ${items[items.length - 1]}`;
}
