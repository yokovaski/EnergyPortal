<script setup lang="ts">
import {computed} from 'vue';

// A number with its unit in a smaller muted span; `euro` puts € in front in the same style ("−€ 1,00" for negatives).
const props = withDefaults(defineProps<{ value: string; unit?: string; euro?: boolean; size?: 'hero' | 'lg' | 'md' }>(), {size: 'lg'});
const negative = computed(() => props.euro && props.value.startsWith('−'));
</script>

<template>
  <span :class="['whitespace-nowrap', `ez-num-${size}`]">
    <template v-if="euro">{{ negative ? '−' : '' }}<span class="ez-unit !ml-0 mr-[.3em]">€</span>{{ negative ? value.slice(1) : value }}</template>
    <template v-else>{{ value }}</template><span v-if="unit" class="ez-unit">{{ unit }}</span>
  </span>
</template>
