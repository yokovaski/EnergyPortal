using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;

namespace EnergyPortal
{
    public static class Config
    {
        public static IEnumerable<Client> GetClients()
        {
            return new[]
            {
                // SPA client using code flow + pkce
                new Client
                {
                    ClientId = "EnergyPortalWebApp",
                    ClientName = "Vue web app",
                    ClientUri = "http://localhost:5000",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = false,

                    RedirectUris =
                    {
                        "http://localhost:5000",
                        "http://localhost:5000/callback",
                        "http://localhost:5000/silent",
                        "http://localhost:5000/popup",
                    },

                    PostLogoutRedirectUris = { "http://localhost:5000" },

                    AllowedScopes = { "openid", "profile", "EnergyPortalAPI" }
                }
            };
        }
    }
}