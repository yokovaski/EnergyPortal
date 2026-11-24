<script setup lang="ts">
import {computed} from 'vue';
import Figure from './Figure.vue';

const props = defineProps<{
  label: string;
  color: string;                 // series background class, e.g. bg-verbruik
  value: number | null;          // null while loading
  format: (v: number) => string; // the number only, without unit
  unit?: string;
  euro?: boolean;
  yesterday?: number | null;     // same time yesterday; no bar when unknown
}>();

const scale = computed(() => Math.max(props.value ?? 0, props.yesterday ?? 0) * 1.12 || 1);
const caption = computed(() => {
  if (props.value == null || props.yesterday == null) return '';
  const diff = props.value - props.yesterday;
  const text = props.format(Math.abs(diff));
  if (!/[1-9]/.test(text)) return 'Gelijk aan gisteren';
  const amount = props.euro ? `€ ${text}` : `${text} ${props.unit}`;
  return `${amount} ${diff > 0 ? 'meer' : 'minder'} dan gisteren`;
});
</script>

<template>
  <div class="min-w-0">
    <div class="ez-label flex items-center gap-2"><span class="ez-swatch" :class="color"/>{{ label }}</div>
    <div class="mt-2">
      <span v-if="value == null" class="ez-skel inline-block h-8 w-24 align-middle"/>
      <Figure v-else :value="format(value)" :unit="unit" :euro="euro"/>
    </div>
    <template v-if="value != null && yesterday != null">
      <div class="relative mt-3 h-1.5 rounded-full bg-surface-sunken" aria-hidden="true">
        <div class="h-full rounded-full" :class="color" :style="{ width: `${(Math.max(value, 0) / scale) * 100}%` }"/>
        <div class="absolute -top-1 h-3.5 w-0.5 bg-ink" :style="{ left: `calc(${(Math.max(yesterday, 0) / scale) * 100}% - 1px)` }"/>
      </div>
      <p class="ez-caption mt-2 mb-0">{{ caption }}</p>
    </template>
  </div>
</template>
