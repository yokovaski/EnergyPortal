import Vue from 'vue'
import VueRouter from 'vue-router'
import Home from '../views/Home.vue'

Vue.use(VueRouter)

const routes = [
  {
    path: '/',
    name: 'Actueel',
    component: Home
  },
  {
    path: '/history',
    name: 'Historie',
    // route level code-splitting
    // this generates a separate chunk (history.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () => import('../views/History.vue')
  },
  {
    path: '/settings',
    name: 'Instellingen',
    component: () => import('../views/Settings.vue'),
  },
  {
    path: '/about',
    name: 'About',
    component: () => import('../views/About.vue')
  },
  {
    path: '/profile',
    name: 'Profiel',
    component: () => import('../views/Profile.vue'),
  },
  {
    path: '/dashboards',
    name: 'Dashboards',
    component: () => import('../views/CustomDashboard.vue'),
  },
]

const router = new VueRouter({
  mode: 'history',
  base: process.env.BASE_URL,
  routes
})

export default router
