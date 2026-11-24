<script setup lang="ts">
import Figure from './Figure.vue';

defineProps<{ label: string; color: string; line?: boolean; value: string | null; unit?: string; euro?: boolean }>();
</script>

<template>
  <div>
    <div class="ez-label flex items-center gap-2">
      <span :class="[line ? 'ez-swatch-line' : 'ez-swatch', color]"/>{{ label }}
    </div>
    <span v-if="value == null" class="ez-skel mt-1 inline-block h-6 w-16"/>
    <Figure v-else size="md" :value="value" :unit="unit" :euro="euro"/>
  </div>
</template>
