import {onMounted, onUnmounted, ref, type Ref} from 'vue';

/** Reactive media query, e.g. useMedia('(max-width: 767px)'). */
export function useMedia(query: string): Ref<boolean> {
    const mql = window.matchMedia(query);
    const matches = ref(mql.matches);
    const update = () => (matches.value = mql.matches);
    onMounted(() => mql.addEventListener('change', update));
    onUnmounted(() => mql.removeEventListener('change', update));
    return matches;
}

/** Closes a popover on outside click or Escape. */
export function useDismiss(root: Ref<HTMLElement | null>, open: Ref<boolean>) {
    const onClick = (e: MouseEvent) => {
        if (open.value && root.value && !root.value.contains(e.target as Node)) open.value = false;
    };
    const onKey = (e: KeyboardEvent) => {
        if (open.value && e.key === 'Escape') open.value = false;
    };
    onMounted(() => {
        document.addEventListener('mousedown', onClick);
        document.addEventListener('keydown', onKey);
    });
    onUnmounted(() => {
        document.removeEventListener('mousedown', onClick);
        document.removeEventListener('keydown', onKey);
    });
}
