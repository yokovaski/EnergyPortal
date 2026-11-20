<script setup lang="ts">
import {ref, watch} from 'vue';
import {num, parseAmount} from '../lib/format';

const props = withDefaults(defineProps<{
  id: string;
  unit: string;              // "/kWh", "/m³", "/maand"
  digits?: number;
  allowNegative?: boolean;
  state?: 'normal' | 'changed' | 'warning';
  describedby?: string;
  labelledby?: string;
}>(), {digits: 5, state: 'normal'});

const model = defineModel<number>({required: true});
const text = ref(num(model.value, props.digits));
const error = ref('');

// Outside changes (Herstellen, loading) rewrite the text, but not while the user is typing a valid value
watch(model, v => {
  if (parseAmount(text.value) !== v) {
    text.value = num(v, props.digits);
    error.value = '';
  }
});

function onInput() {
  const value = parseAmount(text.value);
  if (value == null) error.value = 'Vul een bedrag in, bijvoorbeeld 0,27205.';
  else if (value < 0 && !props.allowNegative) error.value = 'Bedrag kan niet negatief zijn.';
  else {
    error.value = '';
    model.value = value;
  }
}

function onBlur() {
  if (!error.value) text.value = num(model.value, props.digits);
}
</script>

<template>
  <div>
    <div class="flex h-10 items-center gap-2 rounded-md bg-surface-raised px-3 focus-within:outline-2 focus-within:outline-offset-2 focus-within:outline-focus-ring pointer-coarse:h-11"
         :class="error ? 'border-2 border-danger' : state === 'changed' ? 'border-2 border-brand' : state === 'warning' ? 'border-2 border-warning' : 'border border-line-strong'">
      <span class="text-ink-muted" aria-hidden="true">€</span>
      <input :id="id" v-model="text" inputmode="decimal" autocomplete="off"
             class="min-w-0 flex-1 bg-transparent text-right text-ink outline-none"
             :aria-invalid="!!error" :aria-describedby="[describedby, error ? `${id}-error` : ''].filter(Boolean).join(' ') || undefined"
             :aria-labelledby="labelledby"
             @input="onInput" @blur="onBlur">
      <span class="ez-caption whitespace-nowrap" aria-hidden="true">{{ unit }}</span>
    </div>
    <p v-if="error" :id="`${id}-error`" class="ez-caption mt-1 mb-0 !text-danger">{{ error }}</p>
  </div>
</template>
