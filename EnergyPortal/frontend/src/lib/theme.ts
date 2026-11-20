// Theme is a per-device choice, kept in localStorage. The inline script in _Head.cshtml applies it before paint.
export type ThemeChoice = 'licht' | 'donker' | 'systeem';
const KEY = 'ez-theme';
const dark = window.matchMedia('(prefers-color-scheme: dark)');

export function getTheme(): ThemeChoice {
    try {
        const t = localStorage.getItem(KEY);
        if (t === 'donker' || t === 'systeem') return t;
    } catch { /* storage blocked */ }
    return 'licht';
}

function apply(choice: ThemeChoice) {
    const isDark = choice === 'donker' || (choice === 'systeem' && dark.matches);
    document.documentElement.dataset.theme = isDark ? 'dark' : 'light';
}

/** Applies the choice right away (preview); `persist` stores it. */
export function setTheme(choice: ThemeChoice, persist = true) {
    if (persist) {
        try { localStorage.setItem(KEY, choice); } catch { /* storage blocked */ }
    }
    apply(choice);
}

export function initTheme() {
    apply(getTheme());
    dark.addEventListener('change', () => apply(getTheme()));
}
