<template>
  <div>
    <auto-complete
        v-if="!loading"
        :suggestions="options"
        :pre-filled-value="selected"
        @input="timeZoneSelected"
    ></auto-complete>
  </div>
</template>

<script>
import Axios from 'axios';
import AutoComplete from "./AutoComplete.vue";

export default {
  name: "SetTimeZone",
  components: {AutoComplete},
  props: {
    timeZoneId: {
      required: true,
      type: String
    }
  },
  data() {
    return {
      selected: null,
      options: [],
      loading: true,
    }
  },
  async mounted() {
    await this.fetchTimeZones(this.timeZoneId);
  },
  methods: {
    async fetchTimeZones(selected) {
      try {
        this.loading = true;
        let response = await Axios.get('/webapi/v3/settings/time-zones');
        this.options = response.data.timeZones;
        this.selected = selected || '';
      } catch (e) {
        console.error(e);
      }

      this.loading = false;
    },
    async timeZoneSelected(timeZone) {
      this.$emit('time-zone-selected', timeZone.id);
    }
  }
}
</script>

<style scoped>

</style>