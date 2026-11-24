<script setup lang="ts">
import {mdiChartBar, mdiCog, mdiViewDashboard} from '@mdi/js';
import {ref} from 'vue';
import Icon from './components/Icon.vue';
import Wordmark from './components/Wordmark.vue';
import {useAccount} from './composables/useAccount';
import {useDismiss} from './composables/useUi';

const tabs = [
  {to: '/', label: 'Actueel', icon: mdiViewDashboard},
  {to: '/history', label: 'Historie', icon: mdiChartBar},
  {to: '/settings', label: 'Instellingen', icon: mdiCog},
];

const {initials, account} = useAccount();
const menuOpen = ref(false);
const menu = ref<HTMLElement | null>(null);
useDismiss(menu, menuOpen);
</script>

<template>
  <header class="sticky top-0 z-20 border-b border-line bg-surface-raised">
    <div class="mx-auto flex h-14 max-w-content items-center gap-6 px-8 max-[560px]:px-4">
      <RouterLink to="/" class="flex shrink-0 items-center no-underline" aria-label="EnergieZicht, naar Actueel"><Wordmark/></RouterLink>
      <nav aria-label="Hoofdmenu" class="flex gap-1 max-md:hidden">
        <RouterLink v-for="t in tabs" :key="t.to" v-slot="{ href, navigate, isExactActive }" :to="t.to" custom>
          <a :href="href" :aria-current="isExactActive ? 'page' : undefined" @click="navigate"
             class="flex h-10 items-center gap-2 rounded-full px-4 font-medium no-underline"
             :class="isExactActive ? 'bg-brand-soft text-brand font-semibold' : 'text-ink hover:bg-surface-sunken'">
            <Icon :path="t.icon"/>{{ t.label }}
          </a>
        </RouterLink>
      </nav>
      <div ref="menu" class="relative ml-auto">
        <button type="button" class="flex size-10 items-center justify-center rounded-full bg-brand-soft text-sm font-semibold text-brand max-md:size-11"
                :aria-expanded="menuOpen" aria-haspopup="menu" :aria-label="`Accountmenu${account ? ' van ' + (account.name || account.email) : ''}`"
                @click="menuOpen = !menuOpen">
          {{ initials }}
        </button>
        <div v-if="menuOpen" role="menu" class="absolute right-0 mt-2 w-56 rounded-lg border border-line bg-surface-raised py-2 shadow-float"
             @click="menuOpen = false">
          <p v-if="account" class="ez-caption m-0 truncate px-4 pb-2">{{ account.email }}</p>
          <a role="menuitem" href="/Identity/Account/Manage" class="block px-4 py-2 text-ink no-underline hover:bg-brand-soft">Account</a>
          <RouterLink role="menuitem" to="/settings" class="block px-4 py-2 text-ink no-underline hover:bg-brand-soft">Instellingen</RouterLink>
          <!-- .stop: closing the menu would detach this button before the browser submits the form -->
          <button role="menuitem" type="submit" form="logout-form" @click.stop class="block w-full px-4 py-2 text-left text-ink hover:bg-brand-soft">Uitloggen</button>
        </div>
      </div>
    </div>
  </header>

  <main class="mx-auto max-w-content px-8 pt-8 pb-12 max-[560px]:px-4 max-[560px]:pt-5 max-md:pb-[calc(var(--ez-tabbar-h)+env(safe-area-inset-bottom)+24px)]">
    <RouterView/>
  </main>

  <nav aria-label="Hoofdmenu" class="fixed inset-x-0 bottom-0 z-20 flex border-t border-line bg-surface-raised pb-[env(safe-area-inset-bottom)] md:hidden">
    <RouterLink v-for="t in tabs" :key="t.to" v-slot="{ href, navigate, isExactActive }" :to="t.to" custom>
      <a :href="href" :aria-current="isExactActive ? 'page' : undefined" @click="navigate"
         class="flex h-16 flex-1 flex-col items-center justify-center gap-1 text-xs no-underline"
         :class="isExactActive ? 'font-semibold text-brand' : 'text-ink-muted'">
        <span class="flex h-7 w-14 items-center justify-center rounded-full" :class="isExactActive ? 'bg-brand-soft' : ''">
          <Icon :path="t.icon" :size="22"/>
        </span>
        {{ t.label }}
      </a>
    </RouterLink>
  </nav>
</template>
