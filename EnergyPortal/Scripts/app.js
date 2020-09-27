import './app.scss';

import 'bootstrap';
import 'bootstrap/js/dist/util';
import 'bootstrap/js/dist/alert';

// Vue and top level components
import Vue from 'vue';
import Home from './Vue/Home/Home';
import History from "./Vue/History/History";
import UserSettings from "./Vue/Home/Settings/UserSettings";
import BootstrapVue from "bootstrap-vue"; 

Vue.component('home', Home);
Vue.component('history', History);
Vue.component('user-settings', UserSettings);
Vue.use(BootstrapVue);

import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

// FontAwesome
import { library } from '@fortawesome/fontawesome-svg-core';

// fas icons
import {
    faPowerOff, faCloudDownloadAlt, faEdit, faSyncAlt, faCheckCircle, faCalendar, faLongArrowAltRight, faDownload,
    faCar, faSearchLocation, faList, faExclamationTriangle, faClock, faSortDown
}
    from '@fortawesome/free-solid-svg-icons';

// far icons
import { faCircle, faTrashAlt, faFileExcel, faSquare, faCheckSquare, faImage }
    from '@fortawesome/free-regular-svg-icons';

// fas icons
library.add(faPowerOff, faCloudDownloadAlt, faEdit, faSyncAlt, faCheckCircle, faCalendar, faLongArrowAltRight,
    faDownload, faCar, faSearchLocation, faList, faExclamationTriangle, faClock, faSortDown);
// far icons
library.add(faCircle, faTrashAlt, faFileExcel, faSquare, faCheckSquare, faImage);

Vue.component('font-awesome-icon', FontAwesomeIcon);

//Vue.config.productionTip = false;

new Vue({
    el: '#app'
});