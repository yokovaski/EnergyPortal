import Oidc from "oidc-client";

export const oidcSettings = {
  authority: "https://localhost:5001",
  client_id: "EnergyPortalWebApp",
  redirect_uri: "https://localhost:5001/callback",
  post_logout_redirect_uri: "https://localhost:5001",
  response_type: "code",
  scope: "openid profile EnergyPortalAPI",
}