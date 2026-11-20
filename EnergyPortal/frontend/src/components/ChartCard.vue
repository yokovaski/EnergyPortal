<script setup lang="ts">
// Card with title, caption, actions, legend row, the chart and a footnote; owns the loading and empty states.
withDefaults(defineProps<{
  title: string;
  caption?: string;
  footnote?: string;
  state: 'loading' | 'empty' | 'data';
  emptyText?: string;
  unit?: string;   // y-axis unit, shown above the chart
  height?: number;
}>(), {height: 300});
</script>

<template>
  <section class="ez-card flex h-full min-w-0 flex-col">
    <div class="flex flex-wrap items-start justify-between gap-3">
      <div>
        <h2 class="ez-title">{{ title }}</h2>
        <p v-if="caption" class="ez-caption m-0">{{ caption }}</p>
      </div>
      <slot name="actions"/>
    </div>
    <div v-if="$slots.legend" class="mt-5 flex flex-wrap gap-x-8 gap-y-3"><slot name="legend"/></div>
    <div class="mt-3" :style="{ minHeight: `${height}px` }">
      <div v-if="state === 'loading'" class="ez-skel w-full" :style="{ height: `${height}px` }"/>
      <div v-else-if="state === 'empty'" class="flex flex-col items-center justify-center gap-3 rounded-md bg-surface-sunken p-6 text-center"
           :style="{ height: `${height}px` }">
        <slot name="empty"><p class="m-0 text-ink-muted">{{ emptyText ?? 'Nog geen metingen in deze periode' }}</p></slot>
      </div>
      <template v-else>
        <span v-if="unit" class="ez-caption block pl-1">{{ unit }}</span>
        <slot/>
      </template>
    </div>
    <p v-if="footnote" class="ez-caption mt-auto mb-0 pt-4">{{ footnote }}</p>
  </section>
</template>
