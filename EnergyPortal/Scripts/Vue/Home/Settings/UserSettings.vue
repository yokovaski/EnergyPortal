<template>
  <div v-if="!loading">
    <h2>Dashboard settings</h2>
    <hr>
    <b-form-group>
      <b-row class="my-1" align-v="center">
        <b-col sm="3">
          <label>Solar system</label>
        </b-col>
        <b-col sm="9">
          <b-form-checkbox
              switch
              v-model="solarSystem"
              class="mb-3"
          ></b-form-checkbox>
        </b-col>
      </b-row>
      
      <b-row class="my-1">
        <b-col sm="3">
          <label>Show day name in history view</label>
        </b-col>
        <b-col sm="9">
          <b-form-checkbox
              switch
              v-model="showDayName"
              class="mb-3"
          ></b-form-checkbox>
        </b-col>
      </b-row>

      <b-row class="my-1">
        <b-col sm="3">
          <label>Text</label>
        </b-col>
        <b-col sm="9">
          <set-time-zone
              id="time-zone-input"
              :time-zone-id="timeZoneId"
              v-on:time-zone-selected="timeZoneSelected"
              class="mb-3"
          ></set-time-zone>
        </b-col>
      </b-row>
      
      <h2>Rate settings</h2>
      <hr>
      
      <b-row class="my-1" align-v="center">
        <b-col sm="3">
          <label>Price Electricity</label>
        </b-col>
        <b-col sm="9">
          <b-form-input></b-form-input>
        </b-col>
      </b-row>

      <b-row class="my-1">
        <b-col sm="3">
          <label>Text</label>
        </b-col>
        <b-col sm="9">
          <b-form-input></b-form-input>
        </b-col>
      </b-row>
      
      <b-form-group class="my-1" label-cols="3" label="Price Electricity" label-for="input-electricity">
        <b-form-input id="input-electricity" :value="electricityPrice" type="number" step="0.01"></b-form-input>
      </b-form-group>
      
      <hr class="my-5-3">

      <b-button
          v-show="!saving"
          variant="primary"
          onclick="this.blur()"
          @click="saveSettings"
          class="float-right"
      >
        Save
      </b-button>
      <b-button
          v-show="saving"
          variant="primary"
          disabled
          class="float-right"
      >
        <b-spinner v-show="saving" small type="grow"></b-spinner> Save
      </b-button>
    </b-form-group>


  </div>
</template>

<script>
import SetTimeZone from "./SetTimeZone";
import Axios from "axios";
export default {
  name: "UserSettings",
  components: {SetTimeZone},
  data: () => ({
    solarSystem: false,
    showDayName: false,
    timeZoneId: '',
    loading: true,
    saving: false,
    electricityPrice: ''
  }),
  async mounted() {
    await this.fetchSettings();
  },
  methods: {
    timeZoneSelected(timeZoneId) {
      this.timeZoneId = timeZoneId;
    },
    async fetchSettings() {
      try {
        let response = await Axios.get('/webapi/v3/settings');

        console.log(response);

        this.solarSystem = response.data.solarSystem;
        this.showDayName = response.data.showDayName;
        this.timeZoneId = response.data.timeZoneId;
        this.electricityPrice = response.data.electricityPrice;
      } catch (e) {
        console.error(e);
      } finally {
        this.loading = false;
      }
    },
    async saveSettings() {
      try {
        this.saving = true;

        let settings = {
          solarSystem: this.solarSystem,
          showDayName: this.showDayName,
          timeZoneId: this.timeZoneId
        }

        await Axios.put('/webapi/v3/settings', settings);
        this.makeToast('Successfully saved settings!', 'success');
      } catch (e) {
        console.error(e);
        this.makeToast('Failed to save settings.', 'danger');
      } finally {
        this.saving = false;
      }
    },
    makeToast(message, variant = null) {
      this.$bvToast.toast(message, {
        variant: variant,
        solid: true
      })
    }
  }
}
</script>

<style scoped>

</style>