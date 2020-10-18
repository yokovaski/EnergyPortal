import Vue from "vue";
import Vuex from "vuex";
// import { vuexOidcCreateStoreModule } from 'vuex-oidc'
// import { oidcSettings } from './config/oidc'

// let oidcSettings = {
//   "authority": "https://accounts.google.com", 
//   "clientId": "459300396575-3ruj8l8jn69pcgst8rgkqnk6g43gbc78.apps.googleusercontent.com", 
//   "clientName": "SpaTestProject",
//   "redirectUri": "http://localhost:5000/oidc-callback", 
//   "popupRedirectUri": "http://localhost:5000/oidc-popup-callback", 
//   "responseType": "id_token token", 
//   "scope": "openid profile EnergyPortalAPI", 
//   "automaticSilentRenew": true, 
//   "automaticSilentSignin": false, 
//   "silentRedirectUri": "http://localhost:5000/silent-renew-oidc.html"
// }

// let oidcSettings = {
//     "authority": "https://localhost:5001",
//     "client_id": "EnergyPortalWebApp",
//     "redirect_uri": "https://localhost:5001/authentication/login-callback",
//     "post_logout_redirect_uri": "https://localhost:5001/authentication/logout-callback",
//     "response_type": "id_token token",
//     "scope": "EnergyPortalAPI openid profile"
// }

Vue.use(Vuex);

export default new Vuex.Store({
  state: {},
  mutations: {},
  actions: {},
  modules: {}
});
