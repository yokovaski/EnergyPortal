<template>
  <div>
    <home-dashboard v-if="url === ''"></home-dashboard>
  </div>
</template>

<script>


// import Home from "./Home/Home.vue";
// import History from "./History/History.vue";
// import UserSettings from "./Home/Settings/UserSettings.vue";
import HomeDashboard from "./Home/HomeDashboard.vue";

export default {
  name: 'app',
  components: {
    HomeDashboard,
    // Home,
    // History,
    // UserSettings
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