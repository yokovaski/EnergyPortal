<script setup lang="ts">
import {mdiWeatherNight, mdiWeatherSunny} from '@mdi/js';
import Icon from './Icon.vue';

// P1 mode: 1 = dal, 2 = normaal
defineProps<{ mode: number }>();
</script>

<template>
  <span v-if="mode === 1" class="ez-label inline-flex items-center gap-1 rounded-sm bg-dal-bg px-2 py-0.5 text-dal-ink">
    <Icon :path="mdiWeatherNight" :size="14"/>Daltarief
  </span>
  <span v-else-if="mode === 2" class="ez-label inline-flex items-center gap-1 rounded-sm bg-normaal-bg px-2 py-0.5 text-normaal-ink">
    <Icon :path="mdiWeatherSunny" :size="14"/>Normaaltarief
  </span>
</template>
