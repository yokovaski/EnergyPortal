import {defineConfig} from 'vitest/config';

// Separate from vite.config.ts so tests don't need the dev certificate.
export default defineConfig({
    test: {include: ['frontend/**/*.test.ts']},
});
