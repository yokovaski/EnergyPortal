// All number/date formatting for the UI. Locale nl-NL, dates in the user's time zone.
const LOCALE = 'nl-NL';
const MINUS = '−';

const numberFormats = new Map<number, Intl.NumberFormat>();

/** Fixed decimals, Dutch separators, real minus sign: num(-1234.5, 1) = "−1.234,5". */
export function num(value: number, digits: number): string {
    let f = numberFormats.get(digits);
    if (!f) {
        f = new Intl.NumberFormat(LOCALE, {minimumFractionDigits: digits, maximumFractionDigits: digits});
        numberFormats.set(digits, f);
    }
    const text = f.format(Math.abs(value));
    // -0,000 after rounding is shown as 0,000
    return value < 0 && /[1-9]/.test(text) ? MINUS + text : text;
}

export const kw = (v: number) => `${num(v, 3)} kW`;
export const kwh = (v: number) => `${num(v, 1)} kWh`;
export const m3 = (v: number) => `${num(v, 2)} m³`;
/** "€ 1,87", credit "−€ 1,87". */
export function euro(v: number, digits = 2): string {
    const text = num(Math.abs(v), digits);
    return v < 0 && /[1-9]/.test(text) ? `${MINUS}€ ${text}` : `€ ${text}`;
}
export const tariff = (v: number) => euro(v, 4);
export const meter = (v: number) => num(v, 3);

// ---- dates -------------------------------------------------------------------------------------

const dateFormats = new Map<string, Intl.DateTimeFormat>();

function dtf(timeZone: string, options: Intl.DateTimeFormatOptions): Intl.DateTimeFormat {
    const key = timeZone + JSON.stringify(options);
    let f = dateFormats.get(key);
    if (!f) {
        f = new Intl.DateTimeFormat(LOCALE, {timeZone, ...options});
        dateFormats.set(key, f);
    }
    return f;
}

function part(d: Date, tz: string, options: Intl.DateTimeFormatOptions, type: Intl.DateTimeFormatPartTypes): string {
    const value = dtf(tz, options).formatToParts(d).find(p => p.type === type)?.value ?? '';
    return value.replace(/\.$/, '');
}

export interface ZonedParts { year: number; month: number; day: number; hour: number; minute: number; second: number; weekday: number }

/** Calendar fields of an instant in a time zone. weekday: 0 = maandag … 6 = zondag. */
export function zoned(d: Date, tz: string): ZonedParts {
    const parts = dtf(tz, {
        year: 'numeric', month: 'numeric', day: 'numeric', hour: 'numeric', minute: 'numeric', second: 'numeric',
        hourCycle: 'h23', weekday: 'short',
    }).formatToParts(d);
    const get = (t: Intl.DateTimeFormatPartTypes) => Number(parts.find(p => p.type === t)?.value);
    const wd = parts.find(p => p.type === 'weekday')?.value.replace(/\.$/, '') ?? '';
    return {
        year: get('year'), month: get('month'), day: get('day'), hour: get('hour') % 24, minute: get('minute'), second: get('second'),
        weekday: WEEKDAYS.indexOf(wd),
    };
}

const WEEKDAYS = ['ma', 'di', 'wo', 'do', 'vr', 'za', 'zo'];

/** The instant of a wall-clock time in a time zone (DST-safe for normal times). */
export function fromZoned(tz: string, year: number, month: number, day: number, hour = 0, minute = 0): Date {
    const guess = Date.UTC(year, month - 1, day, hour, minute);
    const offset = (t: number) => {
        const p = zoned(new Date(t), tz);
        return Date.UTC(p.year, p.month - 1, p.day, p.hour, p.minute, p.second) - t;
    };
    let t = guess - offset(guess);
    t = guess - offset(t);
    return new Date(t);
}

/** Midnight (start of the day) of the given instant in tz. */
export function startOfDay(d: Date, tz: string): Date {
    const p = zoned(d, tz);
    return fromZoned(tz, p.year, p.month, p.day);
}

/** Adds calendar days in tz, keeping wall-clock midnight across DST. */
export function addDays(d: Date, days: number, tz: string): Date {
    const p = zoned(d, tz);
    return fromZoned(tz, p.year, p.month, p.day + days, p.hour, p.minute);
}

const pad = (n: number) => String(n).padStart(2, '0');

/** "13:42" or with seconds "13:42:30". */
export function time(d: Date, tz: string, seconds = false): string {
    const p = zoned(d, tz);
    return `${pad(p.hour)}:${pad(p.minute)}${seconds ? ':' + pad(p.second) : ''}`;
}

export const weekday = (d: Date, tz: string) => WEEKDAYS[zoned(d, tz).weekday];
export const monthShort = (d: Date, tz: string) => part(d, tz, {month: 'short'}, 'month');
export const monthLong = (d: Date, tz: string) => `${part(d, tz, {month: 'long'}, 'month')} ${zoned(d, tz).year}`;
export const year = (d: Date, tz: string) => String(zoned(d, tz).year);

/** "do 19 nov", or "19 nov" without the day name. */
export function day(d: Date, tz: string, withName = true): string {
    const p = zoned(d, tz);
    const text = `${p.day} ${monthShort(d, tz)}`;
    return withName ? `${WEEKDAYS[p.weekday]} ${text}` : text;
}

/** "13 – 19 nov", "28 okt – 3 nov", "28 dec 2025 – 3 jan 2026". `to` is inclusive. */
export function range(from: Date, to: Date, tz: string): string {
    const a = zoned(from, tz), b = zoned(to, tz);
    if (a.year !== b.year) return `${day(from, tz, false)} ${a.year} – ${day(to, tz, false)} ${b.year}`;
    if (a.month !== b.month) return `${day(from, tz, false)} – ${day(to, tz, false)}`;
    if (a.day === b.day) return day(from, tz, false);
    return `${a.day} – ${day(to, tz, false)}`;
}

/** dd-mm-jjjj for date inputs. */
export function dmy(d: Date, tz: string): string {
    const p = zoned(d, tz);
    return `${pad(p.day)}-${pad(p.month)}-${p.year}`;
}

/** Parses dd-mm-jjjj (also d-m-jjjj, / or . separators) as midnight in tz; null when invalid. */
export function parseDmy(text: string, tz: string): Date | null {
    const m = /^\s*(\d{1,2})[-/.](\d{1,2})[-/.](\d{4})\s*$/.exec(text);
    if (!m) return null;
    const [dd, mm, yy] = [Number(m[1]), Number(m[2]), Number(m[3])];
    const check = new Date(Date.UTC(yy, mm - 1, dd));
    if (check.getUTCMonth() !== mm - 1 || check.getUTCDate() !== dd) return null;
    return fromZoned(tz, yy, mm, dd);
}

/** Accepts "0,272", "0.272", "€ 1.234,50"; returns null when not a number. */
export function parseAmount(text: string): number | null {
    let t = text.replace(/[€\s]/g, '').replace(MINUS, '-');
    if (t === '') return null;
    // with both separators, the last one is the decimal separator
    if (t.includes(',') && t.includes('.')) {
        t = t.lastIndexOf(',') > t.lastIndexOf('.') ? t.replace(/\./g, '').replace(',', '.') : t.replace(/,/g, '');
    } else {
        t = t.replace(',', '.');
    }
    if (!/^-?\d*\.?\d+$/.test(t) && !/^-?\d+\.?$/.test(t)) return null;
    return Number(t);
}
