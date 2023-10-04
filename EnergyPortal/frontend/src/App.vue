<template>
  <div>
    <home v-if="url === ''"></home>
    <history v-else-if="url === 'history'"></history>
    <user-settings v-else-if="url === 'home/settings'"></user-settings>
  </div>
</template>

<script>


import Home from "./Home/Home.vue";
import History from "./History/History.vue";
import UserSettings from "./Home/Settings/UserSettings.vue";

export default {
  name: 'app',
  components: {
    Home,
    History,
    UserSettings
  },
  data() {
    return {
      url: ''
    }
  },
  /**
   * Get the current URL in the browser, which determines which Vue component is shown.
   * @returns {Promise<void>}
   */
  async mounted() {
    this.url = window.location.pathname.slice(1);
  }
}
</script>