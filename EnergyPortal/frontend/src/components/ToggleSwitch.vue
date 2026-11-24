<script setup lang="ts">
defineProps<{ labelledby?: string; describedby?: string }>();
const model = defineModel<boolean>({required: true});
</script>

<template>
  <button type="button" role="switch" :aria-checked="model" :aria-labelledby="labelledby" :aria-describedby="describedby"
          class="relative h-7 w-12 shrink-0 rounded-full border transition-colors pointer-coarse:h-8 pointer-coarse:w-[52px]"
          :class="model ? 'border-brand bg-brand' : 'border-line-strong bg-surface-sunken'"
          @click="model = !model">
    <span class="absolute top-1/2 size-5 -translate-y-1/2 rounded-full transition-[left] pointer-coarse:size-6"
          :class="model ? 'left-[calc(100%-23px)] bg-on-brand pointer-coarse:left-[calc(100%-27px)]' : 'left-[3px] bg-ink-muted'"/>
  </button>
</template>
