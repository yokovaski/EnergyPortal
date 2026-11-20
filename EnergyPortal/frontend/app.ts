import {createApp} from 'vue';
import './src/app.css';
import App from './src/App.vue';
import {router} from './src/router';
import {initTheme} from './src/lib/theme';

initTheme();
createApp(App).use(router).mount('#app');
