<script setup lang="ts">
import {mdiAlertCircleOutline} from '@mdi/js';
import Icon from './Icon.vue';

defineProps<{ variant: 'warning' | 'danger' }>();
</script>

<template>
  <div role="alert" class="flex items-start gap-3 rounded-lg px-4 py-3"
       :class="variant === 'danger' ? 'bg-danger-bg' : 'bg-warning-bg'">
    <span :class="variant === 'danger' ? 'text-danger' : 'text-warning'" class="mt-px"><Icon :path="mdiAlertCircleOutline"/></span>
    <div class="flex flex-1 flex-wrap items-center justify-between gap-3 text-ink"><p class="m-0"><slot/></p><slot name="action"/></div>
  </div>
</template>
