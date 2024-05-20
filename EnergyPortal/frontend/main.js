import { createApp } from 'vue'
import App from './src/App.vue'

// Vuetify

import 'vuetify/styles'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'
import { aliases, mdi } from 'vuetify/iconsets/mdi'
import VueApexCharts from "vue3-apexcharts";
import DateFnsAdapter from '@date-io/date-fns'
import nlNL from 'date-fns/locale/nl'

const vuetify = createVuetify({
    components,
    directives,
    icons: {
        defaultSet: 'mdi',
        aliases,
        sets: {
            mdi,
        },
    },
    date: {
        adapter: DateFnsAdapter,
        locale: {
            en: nlNL,
            nl: nlNL
        },
    },
})

const app = createApp(App);
app.use(vuetify);
app.use(VueApexCharts);
app.mount('#app');
