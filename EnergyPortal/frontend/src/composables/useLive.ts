import {computed, onMounted, onUnmounted, ref} from 'vue';
import {ApiError, get, parseUtc, type Reading} from '../lib/api';

export const POLL_MS = 10_000;
/** Older than this the indicator says "Vertraagd". */
export const STALE_AFTER_MS = 30_000;
/** Older than this (or when the request fails) the Pi counts as offline. */
export const OFFLINE_AFTER_MS = 5 * 60_000;

export type LiveStatus = 'loading' | 'live' | 'stale' | 'offline' | 'none';

const reading = ref<Reading | null>(null);
const failed = ref(false);
const loaded = ref(false);
const noReadings = ref(false);
const now = ref(Date.now());
let users = 0;
let pollTimer: number | undefined;
let clockTimer: number | undefined;

async function poll() {
    try {
        reading.value = await get<Reading>('/webapi/v3/energy/totals');
        failed.value = false;
        noReadings.value = false;
    } catch (e) {
        // 404 = no Pi or no readings at all yet
        if (e instanceof ApiError && e.status === 404) noReadings.value = true;
        else failed.value = true;
    } finally {
        loaded.value = true;
    }
}

/** Latest meter reading, polled every 10 s while any component uses it. */
export function useLive() {
    onMounted(() => {
        if (users++ === 0) {
            void poll();
            pollTimer = window.setInterval(poll, POLL_MS);
            clockTimer = window.setInterval(() => (now.value = Date.now()), 1000);
        }
    });
    onUnmounted(() => {
        if (--users === 0) {
            clearInterval(pollTimer);
            clearInterval(clockTimer);
        }
    });

    const created = computed(() => (reading.value ? parseUtc(reading.value.created) : null));
    const status = computed<LiveStatus>(() => {
        if (!loaded.value) return 'loading';
        if (noReadings.value && !reading.value) return 'none';
        if (!created.value) return 'offline';
        const age = now.value - created.value.getTime();
        if (failed.value || age > OFFLINE_AFTER_MS) return 'offline';
        return age > STALE_AFTER_MS ? 'stale' : 'live';
    });

    return {reading, created, status, refresh: poll};
}
