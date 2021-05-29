<template>
    <div>
        <v-row>
            <v-col cols="12">
                <v-card elevation="3">
                    <v-card-title>Instellingen</v-card-title>
                    <v-card-text>
                        <v-form
                            ref="generalSettings"
                            v-model="valid"
                            lazy-validation
                        >
                            <v-switch
                                v-model="generalSettings.solarSystem"
                                label="Systeem met zonnepanelen"
                            ></v-switch>
                            <v-switch
                                v-model="generalSettings.showDayName"
                                label="Toon dagnaam in grafiek"
                            ></v-switch>
                            <v-autocomplete
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
                            text
                        >
                            Reset
                        </v-btn>
                        <v-btn
                            :disabled="!generalSettingsChanged"
                            color="success"
                            @click="saveSettings"
                            text
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
        timeZoneOptions: [],
        highUsagePricePerKwh: 0.0 ,
        lowUsagePricePerKwh: 0.0 ,
        highRedeliveryPricePerKwh: 0.0 ,
        lowRedeliveryPricePerKwh: 0.0 ,
        gasPrice: 0.0 ,
        electricityDeliveryPricePerMonth: 0.0 ,
        gasDeliveryPricePerMonth: 0.0 ,
        
        valid: true,
        name: '',
        nameRules: [
            v => !!v || 'Name is required',
            v => (v && v.length <= 10) || 'Name must be less than 10 characters',
        ],
        email: '',
        emailRules: [
            v => !!v || 'E-mail is required',
            v => /.+@.+\..+/.test(v) || 'E-mail must be valid',
        ],
        select: null,
        items: [
            'Item 1',
            'Item 2',
            'Item 3',
            'Item 4',
        ],
        checkbox: false,
    }),
    computed: {
        generalSettingsChanged() {
            for (const [key, value] of Object.entries(this.originalSettings)) {
                if (this.generalSettings[key] !== value) {
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
        async saveSettings() {
            if (!this.$refs.generalSettings.validate()) {
                return;
            }
            
            try {
                this.saving = true;

                let settings = {
                    solarSystem: this.generalSettings.solarSystem,
                    showDayName: this.generalSettings.showDayName,
                    timeZoneId: this.generalSettings.selectedTimeZone,
                    highUsagePricePerKwh: this.highUsagePricePerKwh,
                    lowUsagePricePerKwh: this.lowUsagePricePerKwh,
                    highRedeliveryPricePerKwh: this.highRedeliveryPricePerKwh,
                    lowRedeliveryPricePerKwh: this.lowRedeliveryPricePerKwh,
                    gasPrice: this.gasPrice,
                    electricityDeliveryPricePerMonth: this.electricityDeliveryPricePerMonth,
                    gasDeliveryPricePerMonth: this.gasDeliveryPricePerMonth,
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