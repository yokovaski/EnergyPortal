import Vue from "vue";
import VueRouter from "vue-router";
import Home from "../views/Home.vue";
import OidcCallback from "../views/OidcCallback";
import { vuexOidcCreateRouterMiddleware } from "vuex-oidc";
import store from "../store";

Vue.use(VueRouter);

const routes = [
  {
    path: "/",
    name: "Home",
    component: Home
  },
  {
    path: "/about",
    name: "About",
    meta: {
      requiresAuth: true
    },
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () =>
      import(/* webpackChunkName: "about" */ "../views/About.vue")
  },
  {
    path: "/callback",
    name: "oidcCallback",
    component: OidcCallback
  }
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes
});

router.beforeEach(vuexOidcCreateRouterMiddleware(store));

export default router;
