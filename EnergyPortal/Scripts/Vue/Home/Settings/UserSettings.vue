<template>
  <div v-if="!loading">
    <h2>Instellingen</h2>
    <hr>
    <b-form-group>
      <b-row class="my-1" align-v="center">
        <b-col sm="3">
          <label>Systeem met zonnepanelen</label>
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
          <label>Toon dagnaam in grafiek</label>
        </b-col>
        <b-col sm="9">
          <b-form-checkbox
              switch
              v-model="showDayName"
              class="mb-3"
          ></b-form-checkbox>
        </b-col>
      </b-row>

      <b-form-group class="my-1" label-cols="3" label="Tijdszone" label-for="time-zone-input">
        <set-time-zone
            id="time-zone-input"
            :time-zone-id="timeZoneId"
            v-on:time-zone-selected="timeZoneSelected"
            class="mb-3"
        ></set-time-zone>
      </b-form-group>
      
      <h2>Tarieven</h2>
      <hr>
      
      <b-form-group class="my-1" label-cols="3" label="Opname normaaltarief" label-for="input-electricity">
        <b-form-input id="input-electricity" :value="highUsagePricePerKwh" type="number" step="0.01"></b-form-input>
      </b-form-group>
      
      <b-form-group class="my-1" label-cols="3" label="Opname daltarief" label-for="input-electricity">
        <b-form-input id="input-electricity" :value="lowUsagePricePerKwh" type="number" step="0.01"></b-form-input>
      </b-form-group>
      
      <b-form-group class="my-1" label-cols="3" label="Teruglevering normaaltarief" label-for="input-electricity">
        <b-form-input id="input-electricity" :value="highRedeliveryPricePerKwh" type="number" step="0.01"></b-form-input>
      </b-form-group>
      
      <b-form-group class="my-1" label-cols="3" label="Teruglevering daltarief" label-for="input-electricity">
        <b-form-input id="input-electricity" :value="lowRedeliveryPricePerKwh" type="number" step="0.01"></b-form-input>
      </b-form-group>
      
      <b-form-group class="my-1" label-cols="3" label="Gastarief" label-for="input-electricity">
        <b-form-input id="input-electricity" :value="gasPrice" type="number" step="0.01"></b-form-input>
      </b-form-group>
      
      <b-form-group class="my-1" label-cols="3" label="Vaste kosten electriciteit / maand" label-for="input-electricity">
        <b-form-input id="input-electricity" :value="electricityDeliveryPricePerMonth" type="number" step="0.01"></b-form-input>
      </b-form-group>
      
      <b-form-group class="my-1" label-cols="3" label="Vaste kosten gas / maand" label-for="input-electricity">
        <b-form-input id="input-electricity" :value="gasDeliveryPricePerMonth" type="number" step="0.01"></b-form-input>
      </b-form-group>
      

      <b-button
          v-show="!saving"
          variant="primary"
          onclick="this.blur()"
          @click="saveSettings"
          class="float-right mt-3"
      >
        Opslaan
      </b-button>
      <b-button
          v-show="saving"
          variant="primary"
          disabled
          class="float-right mt-3"
      >
        <b-spinner v-show="saving" small type="grow"></b-spinner> Opslaan
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
    highUsagePricePerKwh: 0.00,
    lowUsagePricePerKwh: 0.00,
    highRedeliveryPricePerKwh: 0.00,
    lowRedeliveryPricePerKwh: 0.00,
    gasPrice: 0.00,
    electricityDeliveryPricePerMonth: 0.00,
    gasDeliveryPricePerMonth: 0.00,
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
        this.highUsagePricePerKwh = response.data.highUsagePricePerKwh;
        this.lowUsagePricePerKwh = response.data.lowUsagePricePerKwh;
        this.highRedeliveryPricePerKwh = response.data.highRedeliveryPricePerKwh;
        this.lowRedeliveryPricePerKwh = response.data.lowRedeliveryPricePerKwh;
        this.gasPrice = response.data.gasPrice;
        this.electricityDeliveryPricePerMonth = response.data.electricityDeliveryPricePerMonth;
        this.gasDeliveryPricePerMonth = response.data.gasDeliveryPricePerMonth;
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
          timeZoneId: this.timeZoneId,
          highUsagePricePerKwh: this.highUsagePricePerKwh,
          lowUsagePricePerKwh: this.lowUsagePricePerKwh,
          highRedeliveryPricePerKwh: this.highRedeliveryPricePerKwh,
          lowRedeliveryPricePerKwh: this.lowRedeliveryPricePerKwh,
          gasPrice: this.gasPrice,
          electricityDeliveryPricePerMonth: this.electricityDeliveryPricePerMonth,
          gasDeliveryPricePerMonth: this.gasDeliveryPricePerMonth,
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