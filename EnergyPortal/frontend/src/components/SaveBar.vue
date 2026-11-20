<script setup lang="ts">
import {mdiCheck, mdiLoading} from '@mdi/js';
import Icon from './Icon.vue';

defineProps<{ changes: number; saving?: boolean; saved?: boolean; error?: string }>();
defineEmits<{ save: []; reset: [] }>();
</script>

<template>
  <div class="sticky bottom-4 z-10 max-md:bottom-[calc(var(--ez-tabbar-h)+env(safe-area-inset-bottom)+12px)]" aria-live="polite">
    <div v-if="changes > 0" class="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-line bg-surface-raised px-5 py-3 shadow-float">
      <p class="m-0">
        <template v-if="error"><span class="text-danger">{{ error }}</span></template>
        <template v-else>Je hebt {{ changes }} {{ changes === 1 ? 'wijziging die nog niet is' : 'wijzigingen die nog niet zijn' }} opgeslagen.</template>
      </p>
      <div class="flex gap-2">
        <button type="button" class="ez-btn" :disabled="saving" @click="$emit('reset')">Herstellen</button>
        <button type="button" class="ez-btn ez-btn-primary" :disabled="saving" :aria-busy="saving" @click="$emit('save')">
          <Icon v-if="saving" :path="mdiLoading" :size="18" class="animate-spin"/>{{ saving ? 'Opslaan…' : 'Opslaan' }}
        </button>
      </div>
    </div>
    <!-- Same card as the bar, so it never sits bare on top of the fields -->
    <p v-else-if="saved" class="ez-label m-0 ml-auto flex w-fit items-center gap-1 rounded-lg border border-line bg-surface-raised px-5 py-3 text-live shadow-float">
      <Icon :path="mdiCheck" :size="18"/>Instellingen opgeslagen.
    </p>
  </div>
</template>
