<template>
    <div v-if="!loading">
        <h2>Settings</h2>
        <hr>
        <b-form-group>
            <b-form-checkbox 
                switch
                v-model="solarSystem"
                class="mb-3"
            >
                Solar system
            </b-form-checkbox>
            <b-form-checkbox 
                switch
                v-model="showDayName"
                class="mb-3"
            >
                Show day name in history view
            </b-form-checkbox>
            <set-time-zone 
                id="time-zone-input"
                :time-zone-id="timeZoneId"
                v-on:time-zone-selected="timeZoneSelected"
                class="mb-3"
            ></set-time-zone>
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
        saving: false
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