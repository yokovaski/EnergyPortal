<script setup lang="ts">
import {mdiHome, mdiSolarPanel, mdiTransmissionTower} from '@mdi/js';
import {computed} from 'vue';
import Figure from '../../components/Figure.vue';
import Icon from '../../components/Icon.vue';
import {useMedia} from '../../composables/useUi';
import type {Flow} from '../../lib/energy';
import {kw, num} from '../../lib/format';

// Power flow: Opwekking → Verbruik → Teruglevering, or Net (Opname) → Verbruik.
const props = defineProps<{ flow: Flow | null; hasSolar: boolean }>();
const mobile = useMedia('(max-width: 767px)');

const toGrid = computed(() => props.hasSolar && (props.flow?.teruglevering ?? 0) > 0);
const grid = computed(() => toGrid.value
    ? {label: 'Teruglevering', ring: 'border-teruglevering', fill: 'bg-teruglevering', value: props.flow?.teruglevering ?? 0}
    : {label: 'Opname', ring: 'border-opname', fill: 'bg-opname', value: props.flow?.opname ?? 0});

const sentence = computed(() => {
  const f = props.flow;
  if (!f) return '';
  if (!props.hasSolar) return 'Alle stroom komt nu van het net.';
  if (f.opwekking <= 0) return `Je panelen leveren nu niets. Opname van het net: ${kw(f.opname)}.`;
  return `Je gebruikt nu ${Math.round((f.direct / f.opwekking) * 100)}% van je zonnestroom zelf. Opname van het net: ${kw(f.opname)}.`;
});
</script>

<template>
  <div v-if="!flow" class="flex flex-col items-center gap-4 py-6" aria-hidden="true">
    <span class="ez-skel size-[88px] !rounded-full"/>
    <span class="ez-skel h-12 w-40"/>
  </div>

  <!-- Mobile: hero value on top, compact nodes in a row -->
  <div v-else-if="mobile">
    <div class="text-center">
      <p class="ez-label m-0">Verbruik</p>
      <Figure size="hero" :value="num(flow.verbruik, 3)" unit="kW"/>
    </div>
    <div class="mt-5 grid items-center gap-1" :class="hasSolar ? 'grid-cols-[1fr_auto_1fr_auto_1fr]' : 'grid-cols-[1fr_auto_1fr]'">
      <template v-if="hasSolar">
        <div class="flex flex-col items-center text-center">
          <span class="flex size-12 items-center justify-center rounded-full border-[3px] border-opwekking"><Icon :path="mdiSolarPanel"/></span>
          <span class="ez-caption mt-1 !text-ink">Opwekking</span>
          <span class="ez-label">{{ kw(flow.opwekking) }}</span>
        </div>
        <span class="text-opwekking" aria-hidden="true">→</span>
      </template>
      <template v-else>
        <div class="flex flex-col items-center text-center">
          <span class="flex size-12 items-center justify-center rounded-full border-[3px] border-opname"><Icon :path="mdiTransmissionTower"/></span>
          <span class="ez-caption mt-1 !text-ink">Opname</span>
          <span class="ez-label">{{ kw(flow.opname) }}</span>
        </div>
        <span class="text-opname" aria-hidden="true">→</span>
      </template>
      <div class="flex flex-col items-center text-center">
        <span class="flex size-12 items-center justify-center rounded-full border-[3px] border-verbruik"><Icon :path="mdiHome"/></span>
        <span class="ez-caption mt-1 !text-ink">Huis</span>
      </div>
      <template v-if="hasSolar">
        <span :class="toGrid ? 'text-teruglevering' : 'text-opname'" aria-hidden="true">{{ toGrid ? '→' : '←' }}</span>
        <div class="flex flex-col items-center text-center">
          <span class="flex size-12 items-center justify-center rounded-full border-[3px]" :class="grid.ring"><Icon :path="mdiTransmissionTower"/></span>
          <span class="ez-caption mt-1 !text-ink">{{ grid.label }}</span>
          <span class="ez-label">{{ kw(grid.value) }}</span>
        </div>
      </template>
    </div>
    <p class="ez-caption mt-4 mb-0 text-center">{{ sentence }}</p>
  </div>

  <!-- Desktop -->
  <div v-else>
    <div class="flex items-start justify-center gap-4 px-4">
      <!-- left node: Opwekking (solar) or Net (no solar) -->
      <div class="flex w-40 flex-col items-center pt-3 text-center">
        <span class="flex size-16 items-center justify-center rounded-full border-[3px]" :class="hasSolar ? 'border-opwekking' : 'border-opname'">
          <Icon :path="hasSolar ? mdiSolarPanel : mdiTransmissionTower" :size="24"/>
        </span>
        <span class="ez-label mt-3">{{ hasSolar ? 'Opwekking' : 'Opname' }}</span>
        <Figure :value="num(hasSolar ? flow.opwekking : flow.opname, 3)" unit="kW"/>
      </div>
      <div class="flex flex-1 flex-col items-center pt-5">
        <span class="ez-caption">{{ hasSolar ? `${kw(flow.direct)} direct gebruikt` : `${kw(flow.opname)} van het net` }}</span>
        <span class="mt-2 flex w-full items-center" :class="hasSolar ? 'text-opwekking' : 'text-opname'" aria-hidden="true">
          <span class="h-[3px] flex-1 bg-current"/><span class="size-0 border-y-[6px] border-l-[10px] border-y-transparent border-l-current"/>
        </span>
      </div>
      <!-- house -->
      <div class="flex w-48 flex-col items-center text-center">
        <span class="flex size-[88px] items-center justify-center rounded-full border-[3px] border-verbruik"><Icon :path="mdiHome" :size="28"/></span>
        <span class="ez-label mt-3">Verbruik</span>
        <span aria-live="off"><Figure size="hero" :value="num(flow.verbruik, 3)" unit="kW"/></span>
      </div>
      <template v-if="hasSolar">
        <div class="flex flex-1 flex-col items-center pt-5">
          <span class="ez-caption">{{ toGrid ? `${kw(grid.value)} naar het net` : `${kw(grid.value)} van het net` }}</span>
          <span class="mt-2 flex w-full items-center" :class="toGrid ? 'text-teruglevering' : 'text-opname'" aria-hidden="true">
            <span v-if="!toGrid" class="size-0 border-y-[6px] border-r-[10px] border-y-transparent border-r-current"/>
            <span class="h-[3px] flex-1 bg-current"/>
            <span v-if="toGrid" class="size-0 border-y-[6px] border-l-[10px] border-y-transparent border-l-current"/>
          </span>
        </div>
        <div class="flex w-40 flex-col items-center pt-3 text-center">
          <span class="flex size-16 items-center justify-center rounded-full border-[3px]" :class="grid.ring"><Icon :path="mdiTransmissionTower" :size="24"/></span>
          <span class="ez-label mt-3">{{ grid.label }}</span>
          <Figure :value="num(grid.value, 3)" unit="kW"/>
        </div>
      </template>
    </div>
    <p class="ez-caption mt-6 mb-0 text-center">{{ sentence }}</p>
  </div>
</template>
