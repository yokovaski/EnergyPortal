import Vue from "vue";
import App from "./App.vue";
import router from "./router";
import store from "./store";
import mgr from "@/security";

Vue.config.productionTip = false;

const globalData = {
  isAuthenticated: false,
  user: '',
  mgr: mgr
}

const globalMethods = {
  async authenticate(returnPath) {
    const user = await this.$root.getUser();
    
    if (!!user) {
      this.authenticated = true;
      this.user = user;
    } else {
      await this.$root.signIn();
    }
  },
  async getUser () {
    try {
      return await this.mgr.getUser();
    } catch (err) {
      console.log(err);
    }
  },
  signIn (returnPath) {
    returnPath 
        ? this.mgr.signinRedirect({ state: returnPath })
        : this.mgr.signinRedirect();
  }
}

new Vue({
  router,
  store,
  data: globalData,
  methods: globalMethods,
  render: h => h(App)
}).$mount("#app");
