import {createRouter, createWebHistory} from 'vue-router';

export const router = createRouter({
    history: createWebHistory(),
    routes: [
        {path: '/', component: () => import('./pages/Actueel.vue'), meta: {title: 'Actueel'}},
        {path: '/history', component: () => import('./pages/Historie.vue'), meta: {title: 'Historie'}},
        {path: '/settings', component: () => import('./pages/Instellingen.vue'), meta: {title: 'Instellingen'}},
        {path: '/home/settings', redirect: '/settings'},
        {path: '/:rest(.*)*', component: () => import('./pages/NietGevonden.vue'), meta: {title: 'Pagina niet gevonden'}},
    ],
});

router.afterEach(to => {
    document.title = `${to.meta.title ?? ''} - EnergieZicht`;
});
