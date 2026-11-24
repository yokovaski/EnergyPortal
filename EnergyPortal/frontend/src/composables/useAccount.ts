import {computed, ref} from 'vue';
import {get, type Account} from '../lib/api';

const account = ref<Account | null>(null);
let loading: Promise<unknown> | null = null;

export function useAccount() {
    loading ??= get<Account>('/webapi/v3/account').then(a => (account.value = a)).catch(() => undefined);
    const initials = computed(() => initialsOf(account.value?.name || account.value?.email || ''));
    return {account, initials};
}

/** "Erwin Lenting" → "EL", "naam@voorbeeld.nl" → "NA". */
export function initialsOf(text: string): string {
    const words = text.split('@')[0].split(/[\s._-]+/).filter(Boolean);
    if (words.length >= 2) return (words[0][0] + words[words.length - 1][0]).toUpperCase();
    return (words[0] ?? '').slice(0, 2).toUpperCase();
}
