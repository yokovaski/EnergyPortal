import 'vite/modulepreload-polyfill'
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-vue/dist/bootstrap-vue.css'

import Vue from 'vue'
import App from './src/App.vue'

/* import the fontawesome core */
import { library } from '@fortawesome/fontawesome-svg-core'
import { BootstrapVue, IconsPlugin } from 'bootstrap-vue'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

Vue.use(BootstrapVue)
Vue.use(IconsPlugin)

/* import specific icons */
import { faSortDown } from '@fortawesome/free-solid-svg-icons'

/* add icons to the library */
library.add(faSortDown)

Vue.component('font-awesome-icon', FontAwesomeIcon);

import './src/style.css'

new Vue({
    render: (h) => h(App)
}).$mount('#app')
