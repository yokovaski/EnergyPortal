<template>
  <div class="about">
    <h1>This is an about page</h1>
    <div v-if="oidcUser">
      Signed in as {{ oidcUser.email }}
      <button @click="signOutOidc">Sign out</button>
    </div>
  </div>
</template>

<script>
import { mapGetters, mapActions } from 'vuex'

export default {
  name: "About",
  computed: {
    ...mapGetters([
      "oidcUser"
    ])
  },
  methods: {
    ...mapActions([
      "signOutOidc"
    ])
  }
}
</script>
