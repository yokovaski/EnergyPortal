// Thin fetch wrapper for the /webapi endpoints (cookie-authenticated).

export class ApiError extends Error {
    constructor(public status: number, message: string) {
        super(message);
    }
}

function loginRedirect() {
    window.location.href = `/Identity/Account/Login?ReturnUrl=${encodeURIComponent(location.pathname + location.search)}`;
}

async function request<T>(method: string, url: string, body?: unknown): Promise<T | null> {
    const response = await fetch(url, {
        method,
        headers: body === undefined ? {Accept: 'application/json'} : {'Content-Type': 'application/json', Accept: 'application/json'},
        body: body === undefined ? undefined : JSON.stringify(body),
    });
    // An expired session redirects API calls to the login page
    if (response.redirected && response.url.includes('/Identity/Account/Login')) {
        loginRedirect();
        throw new ApiError(401, 'Niet ingelogd');
    }
    if (!response.ok) throw new ApiError(response.status, await response.text());
    if (response.status === 204) return null;
    const text = await response.text();
    return text ? JSON.parse(text) as T : null;
}

export const get = <T>(url: string) => request<T>('GET', url);
export const put = (url: string, body: unknown) => request<unknown>('PUT', url, body);

/** Query string with ISO dates. */
export function query(params: Record<string, string | number | boolean | Date | undefined>): string {
    const q = new URLSearchParams();
    for (const [k, v] of Object.entries(params)) {
        if (v === undefined) continue;
        q.set(k, v instanceof Date ? v.toISOString() : String(v));
    }
    return q.toString();
}

// ---- response shapes ----

export interface Settings {
    id?: number;
    timeZoneId: string;
    solarSystem: boolean;
    showDayName: boolean;
    gasPrice: number;
    highUsagePricePerKwh: number;
    lowUsagePricePerKwh: number;
    highRedeliveryPricePerKwh: number;
    lowRedeliveryPricePerKwh: number;
    electricityDeliveryPricePerMonth: number;
    gasDeliveryPricePerMonth: number;
    postcode?: string | null;
    /** Nearest KNMI station for the postcode (read-only). */
    weatherStation?: string | null;
}

/** The latest 10-second reading (GET /webapi/v3/energy/totals). Powers in W, totals in Wh and dm³. */
export interface Reading {
    mode: number;
    usageNow: number;
    redeliveryNow: number;
    solarNow: number;
    usageTotalHigh: number;
    usageTotalLow: number;
    redeliveryTotalHigh: number;
    redeliveryTotalLow: number;
    solarTotal: number;
    usageGasTotal: number;
    created: string;
}

/** kWh and m³ since midnight (GET /webapi/v3/energy/total-today, total-yesterday-until-now). */
export interface DayTotals {
    usageTotalHigh: number;
    usageTotalLow: number;
    redeliveryTotalHigh: number;
    redeliveryTotalLow: number;
    totalSolar: number;
    totalGas: number;
}

type Point = [number, number];

/** GET /webapi/v3/metrics/{group}. Values in kW (instant groups) or kWh/m³ (summed groups), costs in €. */
export interface Series {
    epochs: number[];
    usage: Point[];
    solar: Point[];
    redelivery: Point[];
    intake: Point[];
    gas: Point[];
    usageCosts: Point[];
    intakeCosts: Point[];
    redeliveryCosts: Point[];
    gasCosts: Point[];
    /** Mean outdoor °C and heating degree days per bucket; empty without a postcode, null where unmeasured. */
    temperature: [number, number | null][];
    degreeDays: [number, number | null][];
    userTimeZone: string;
}

export interface Account {
    email: string;
    name: string | null;
    emailConfirmed: boolean;
    twoFactorEnabled: boolean;
    rpiKey: string | null;
}

export type Group = 'tenseconds' | 'minutes' | 'fiveminutes' | 'hours' | 'days' | 'months' | 'years';

export function getSeries(group: Group, from: Date, to?: Date) {
    return get<Series>(`/webapi/v3/metrics/${group}?${query({from, to})}`);
}

/** Server timestamps are UTC; tolerate a missing offset. */
export function parseUtc(value: string): Date {
    return new Date(/[zZ]|[+-]\d\d:?\d\d$/.test(value) ? value : value + 'Z');
}
