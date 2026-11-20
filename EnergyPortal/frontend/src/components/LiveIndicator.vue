<script setup lang="ts">
import {computed} from 'vue';
import type {LiveStatus} from '../composables/useLive';
import {time} from '../lib/format';

const props = defineProps<{ status: LiveStatus; created: Date | null; timeZone: string }>();
const at = computed(() => (props.created ? time(props.created, props.timeZone, true) : ''));
</script>

<template>
  <span class="inline-flex flex-wrap items-center gap-x-2 whitespace-nowrap" role="status">
    <span v-if="status === 'loading'" class="ez-skel inline-block h-4 w-40"/>
    <template v-else-if="status === 'live'">
      <span class="size-2 rounded-full bg-live"/><span class="ez-label">Live</span>
      <span class="ez-caption">bijgewerkt {{ at }}</span>
    </template>
    <template v-else-if="status === 'stale'">
      <span class="size-2 rounded-full bg-warning"/><span class="ez-label">Vertraagd</span>
      <span class="ez-caption">laatste meting {{ at }}</span>
    </template>
    <template v-else>
      <span class="size-2 rounded-full bg-danger"/><span class="ez-label">Offline</span>
      <span v-if="at" class="ez-caption">laatste meting {{ at }}</span>
    </template>
  </span>
</template>
