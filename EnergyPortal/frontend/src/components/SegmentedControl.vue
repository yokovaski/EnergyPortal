<script setup lang="ts" generic="T extends string">
defineProps<{ options: { value: T; label: string }[]; label: string }>();
const model = defineModel<T>({required: true});
</script>

<template>
  <div role="group" :aria-label="label" class="inline-flex rounded-md bg-surface-sunken p-[3px]">
    <button v-for="o in options" :key="o.value" type="button" :aria-pressed="model === o.value"
            class="h-[34px] rounded-sm px-4 font-medium pointer-coarse:h-[38px]"
            :class="model === o.value ? 'bg-brand font-semibold text-on-brand' : 'text-ink hover:bg-brand-soft'"
            @click="model = o.value">
      {{ o.label }}
    </button>
  </div>
</template>
