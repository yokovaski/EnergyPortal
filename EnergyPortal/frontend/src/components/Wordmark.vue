<template>
  <!-- Light and dark logo; app.css shows the one for the theme. Bound src: served from wwwroot, not bundled by Vite. -->
  <img :src="'/img/energiezicht-logo.svg'" alt="" height="20" class="ez-logo-light h-5 w-auto">
  <img :src="'/img/energiezicht-logo-donker.svg'" alt="" height="20" class="ez-logo-dark h-5 w-auto">
</template>
