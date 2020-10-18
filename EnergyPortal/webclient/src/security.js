import Oidc from "oidc-client";

let mgr = new Oidc.UserManager({
  authority: "https://localhost:5001",
  client_id: "EnergyPortalWebApp",
  redirect_uri: "https://localhost:5001/callback",
  post_logout_redirect_uri: "https://localhost:5001",
  response_type: "code",
  scope: "openid profile EnergyPortalAPI",
  userStore: new Oidc.WebStorageStateStore({ store: window.localStorage })
});

Oidc.Log.logger = console;
Oidc.Log.level = Oidc.Log.INFO;

export default mgr;
