<template>
  <div>
    <v-row no-gutters class="pb-3">
      <v-col cols="12">
        <v-card elevation="3">
          <v-card-title>Instellingen</v-card-title>
          <v-card-text>
            <v-form
                ref="generalSettings"
                lazy-validation
            >
              <v-switch
                  color="primary"
                  v-model="generalSettings.solarSystem"
                  label="Systeem met zonnepanelen"
              ></v-switch>
              <v-switch
                  color="primary"
                  v-model="generalSettings.showDayName"
                  label="Toon dagnaam in grafiek"
              ></v-switch>
              <v-autocomplete
                  clearable
                  variant="outlined"
                  label="Tijdszone"
                  v-model="generalSettings.selectedTimeZone"
                  :items="timeZoneOptions"
                  :rules="[v => validateTimeZone(v) || 'Ongeldige tijdszone']"
              ></v-autocomplete>
            </v-form>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn
                :disabled="!generalSettingsChanged"
                color="error"
                class="mr-4"
                @click="resetGeneralSettings"
            >
              Reset
            </v-btn>
            <v-btn
                :disabled="!generalSettingsChanged"
                color="success"
                @click="saveGeneralSettings"
            >
              Opslaan
            </v-btn>
          </v-card-actions>
        </v-card>

      </v-col>
    </v-row>
    <v-row no-gutters>
      <v-col cols="12" class="pt-3 pb-3">
        <v-card elevation="3">
          <v-card-title>Tarieven</v-card-title>
          <v-card-text>
            <v-form
                ref="generalSettings"
                lazy-validation
            >
              <v-text-field
                  clearable
                  variant="outlined"
                  v-model="prices.highUsagePricePerKwh"
                  label="Opname normaaltarief"
                  prefix="€"
                  type="number"
                  step="0.0000000001"
              ></v-text-field>
              <v-text-field
                  clearable
                  variant="outlined"
                  v-model="prices.lowUsagePricePerKwh"
                  label="Opname daltarief"
                  prefix="€"
                  type="number"
                  step="0.0000000001"
              ></v-text-field>
              <v-text-field
                  clearable
                  variant="outlined"
                  v-model="prices.highRedeliveryPricePerKwh"
                  label="Teruglevering normaaltarief"
                  prefix="€"
                  type="number"
                  step="0.0000000001"
              ></v-text-field>
              <v-text-field
                  clearable
                  variant="outlined"
                  v-model="prices.lowRedeliveryPricePerKwh"
                  label="Teruglevering daltarief"
                  prefix="€"
                  type="number"
                  step="0.0000000001"
              ></v-text-field>
              <v-text-field
                  clearable
                  variant="outlined"
                  v-model="prices.gasPrice"
                  label="Gastarief"
                  prefix="€"
                  type="number"
                  step="0.0000000001"
              ></v-text-field>
              <v-text-field
                  clearable
                  variant="outlined"
                  v-model="prices.electricityDeliveryPricePerMonth"
                  label="Vaste kosten electriciteit / maand"
                  prefix="€"
                  type="number"
                  step="0.0000000001"
              ></v-text-field>
              <v-text-field
                  clearable
                  variant="outlined"
                  v-model="prices.gasDeliveryPricePerMonth"
                  label="Vaste kosten gas / maand"
                  prefix="€"
                  type="number"
                  step="0.0000000001"
              ></v-text-field>

            </v-form>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn
                :disabled="!pricesChanged"
                color="error"
                class="mr-4"
                @click="resetPrices"
            >
              Reset
            </v-btn>
            <v-btn
                :disabled="!pricesChanged"
                color="success"
                @click="savePrices"
            >
              Opslaan
            </v-btn>
          </v-card-actions>
        </v-card>

      </v-col>
    </v-row>
  </div>
</template>

<script>
import Axios from "axios";

export default {
  name: "AppSettings",
  data: () => ({
    generalSettings: {
      solarSystem: false,
      showDayName: false,
      selectedTimeZone: null,
    },
    originalSettings: {
      solarSystem: false,
      showDayName: false,
      selectedTimeZone: null,
    },
    prices: {
      highUsagePricePerKwh: 0.0,
      lowUsagePricePerKwh: 0.0,
      highRedeliveryPricePerKwh: 0.0,
      lowRedeliveryPricePerKwh: 0.0,
      gasPrice: 0.0,
      electricityDeliveryPricePerMonth: 0.0,
      gasDeliveryPricePerMonth: 0.0,
    },
    originalPrices: {
      highUsagePricePerKwh: 0.0,
      lowUsagePricePerKwh: 0.0,
      highRedeliveryPricePerKwh: 0.0,
      lowRedeliveryPricePerKwh: 0.0,
      gasPrice: 0.0,
      electricityDeliveryPricePerMonth: 0.0,
      gasDeliveryPricePerMonth: 0.0,
    },
    timeZoneOptions: []
  }),
  computed: {
    generalSettingsChanged() {
      for (const [key, value] of Object.entries(this.originalSettings)) {
        if (this.generalSettings[key] !== value) {
          return true;
        }
      }

      return false;
    },
    pricesChanged() {
      for (const [key, value] of Object.entries(this.originalPrices)) {
        if (this.prices[key] !== value) {
          return true;
        }
      }

      return false;
    }
  },
  async mounted() {
    await this.fetchSettings();
    await this.fetchTimeZones();
  },
  methods: {
    validateTimeZone(t) {
      return !!t || t in this.timeZoneOptions;
    },
    resetGeneralSettings() {
      for (const [key, value] of Object.entries(this.originalSettings)) {
        this.generalSettings[key] = value;
      }

      this.$refs.generalSettings.resetValidation();
    },
    async fetchSettings() {
      try {
        let response = await Axios.get('/webapi/v3/settings');

        this.originalSettings.solarSystem = response.data.solarSystem;
        this.originalSettings.showDayName = response.data.showDayName;
        this.originalSettings.selectedTimeZone = response.data.timeZoneId;
        this.resetGeneralSettings();

        this.originalPrices.highUsagePricePerKwh = response.data.highUsagePricePerKwh;
        this.originalPrices.lowUsagePricePerKwh = response.data.lowUsagePricePerKwh;
        this.originalPrices.highRedeliveryPricePerKwh = response.data.highRedeliveryPricePerKwh;
        this.originalPrices.lowRedeliveryPricePerKwh = response.data.lowRedeliveryPricePerKwh;
        this.originalPrices.gasPrice = response.data.gasPrice;
        this.originalPrices.electricityDeliveryPricePerMonth = response.data.electricityDeliveryPricePerMonth;
        this.originalPrices.gasDeliveryPricePerMonth = response.data.gasDeliveryPricePerMonth;
        this.resetPrices();
      } catch (e) {
        console.error(e);
      } finally {
        this.loading = false;
      }
    },
    async fetchTimeZones() {
      try {
        this.loading = true;
        let response = await Axios.get('/webapi/v3/settings/time-zones');
        this.timeZoneOptions = response.data.timeZones.map(t => t.id);
      } catch (e) {
        console.error(e);
      }

      this.loading = false;
    },
    async saveGeneralSettings() {
      if (!this.$refs.generalSettings.validate()) {
        return;
      }

      try {
        this.saving = true;

        let settings = {
          solarSystem: this.generalSettings.solarSystem,
          showDayName: this.generalSettings.showDayName,
          timeZoneId: this.generalSettings.selectedTimeZone,
          highUsagePricePerKwh: this.originalPrices.highUsagePricePerKwh,
          lowUsagePricePerKwh: this.originalPrices.lowUsagePricePerKwh,
          highRedeliveryPricePerKwh: this.originalPrices.highRedeliveryPricePerKwh,
          lowRedeliveryPricePerKwh: this.originalPrices.lowRedeliveryPricePerKwh,
          gasPrice: this.originalPrices.gasPrice,
          electricityDeliveryPricePerMonth: this.originalPrices.electricityDeliveryPricePerMonth,
          gasDeliveryPricePerMonth: this.originalPrices.gasDeliveryPricePerMonth,
        }

        await Axios.put('/webapi/v3/settings', settings);
        await this.fetchSettings();
        // this.makeToast('Successfully saved settings!', 'success');
      } catch (e) {
        console.error(e);
        // this.makeToast('Failed to save settings.', 'danger');
      } finally {
        this.saving = false;
      }
    },
    resetPrices() {
      for (const [key, value] of Object.entries(this.originalPrices)) {
        this.prices[key] = value;
      }
    },
    async savePrices() {
      if (!this.$refs.generalSettings.validate()) {
        return;
      }

      try {
        this.saving = true;

        let settings = {
          solarSystem: this.originalPrices.solarSystem,
          showDayName: this.originalPrices.showDayName,
          timeZoneId: this.originalPrices.selectedTimeZone,
          highUsagePricePerKwh: this.prices.highUsagePricePerKwh,
          lowUsagePricePerKwh: this.prices.lowUsagePricePerKwh,
          highRedeliveryPricePerKwh: this.prices.highRedeliveryPricePerKwh,
          lowRedeliveryPricePerKwh: this.prices.lowRedeliveryPricePerKwh,
          gasPrice: this.prices.gasPrice,
          electricityDeliveryPricePerMonth: this.prices.electricityDeliveryPricePerMonth,
          gasDeliveryPricePerMonth: this.prices.gasDeliveryPricePerMonth,
        }

        await Axios.put('/webapi/v3/settings', settings);
        await this.fetchSettings();
        // this.makeToast('Successfully saved settings!', 'success');
      } catch (e) {
        console.error(e);
        // this.makeToast('Failed to save settings.', 'danger');
      } finally {
        this.saving = false;
      }
    },
  },
}
</script>

<style scoped>

</style>