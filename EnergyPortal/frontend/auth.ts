// Styles and small behaviour for the server-rendered (Razor) pages: login, account, errors.
import './src/app.css';
import './src/auth.css';
import {initTheme} from './src/lib/theme';

initTheme();

// "Toon / Verberg" buttons inside password fields
document.querySelectorAll<HTMLButtonElement>('[data-toggle-password]').forEach(button => {
    const input = document.getElementById(button.dataset.togglePassword!) as HTMLInputElement | null;
    if (!input) return;
    button.addEventListener('click', () => {
        const show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        button.textContent = show ? 'Verberg' : 'Toon';
        button.setAttribute('aria-pressed', String(show));
    });
});

// Buttons that need a confirmation (e.g. turning off two-step verification)
document.querySelectorAll<HTMLFormElement>('form[data-confirm]').forEach(form => {
    form.addEventListener('submit', e => {
        if (!confirm(form.dataset.confirm!)) e.preventDefault();
    });
});

// Native <dialog> openers: <button data-open-dialog="id">
document.querySelectorAll<HTMLButtonElement>('[data-open-dialog]').forEach(button => {
    button.addEventListener('click', () => (document.getElementById(button.dataset.openDialog!) as HTMLDialogElement | null)?.showModal());
});
document.querySelectorAll<HTMLButtonElement>('[data-close-dialog]').forEach(button => {
    button.addEventListener('click', () => button.closest('dialog')?.close());
});
