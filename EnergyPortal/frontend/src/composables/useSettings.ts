import {ref} from 'vue';
import {get, put, type Settings} from '../lib/api';

export const DEFAULT_SETTINGS: Settings = {
    timeZoneId: 'Europe/Amsterdam', solarSystem: false, showDayName: true,
    gasPrice: 0, highUsagePricePerKwh: 0, lowUsagePricePerKwh: 0, highRedeliveryPricePerKwh: 0, lowRedeliveryPricePerKwh: 0,
    electricityDeliveryPricePerMonth: 0, gasDeliveryPricePerMonth: 0,
};

// Shared by every page: solarSystem hides solar UI everywhere, timeZoneId drives all dates.
const settings = ref<Settings | null>(null);
let loading: Promise<Settings> | null = null;

export function useSettings() {
    if (!loading) {
        loading = get<Settings>('/webapi/v3/settings')
            .then(s => (settings.value = {...DEFAULT_SETTINGS, ...(s ?? {})}))
            .catch(() => (settings.value = {...DEFAULT_SETTINGS}));
    }
    return {
        settings,
        ready: loading,
        async save(next: Settings) {
            await put('/webapi/v3/settings', next);
            // Re-read: the server normalises the postcode and picks the weather station
            settings.value = {...DEFAULT_SETTINGS, ...(await get<Settings>('/webapi/v3/settings') ?? next)};
        },
    };
}
