export const oidcSettings = {
  authority: "http://localhost:5000",
  client_id: "EnergyPortalWebApp",
  redirect_uri: "http://localhost:5000/callback",
  post_logout_redirect_uri: "http://localhost:5000",
  response_type: "code",
  scope: "openid profile EnergyPortalAPI"
};
