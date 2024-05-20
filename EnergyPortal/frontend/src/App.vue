<template>
  <div>
    <home-dashboard v-if="matchesUrl('')"></home-dashboard>
    <history v-else-if="matchesUrl('history')"></history>
    <user-settings v-else-if="matchesUrl('home/settings')"></user-settings>
  </div>
</template>

<script>


import History from "./History/History.vue";
import UserSettings from "./Home/Settings/UserSettings.vue";
import HomeDashboard from "./Home/HomeDashboard.vue";

export default {
  name: 'app',
  components: {
    HomeDashboard,
    History,
    UserSettings
  },
  computed: {
    url() {
      return window.location.pathname.slice(1);
    },
  },
  methods: {
    matchesUrl(urlToMatch) {
      return (this.url.localeCompare(urlToMatch, 'en', {sensitivity: 'accent'}) === 0);
    }
  }
}
</script>

<style>
@import "style.css";
</style>