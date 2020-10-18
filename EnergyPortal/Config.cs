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
                    ClientUri = "https://localhost:5001",

                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = false,

                    RedirectUris =
                    {
                        "https://localhost:5001",
                        "https://localhost:5001/callback",
                        "https://localhost:5001/silent",
                        "https://localhost:5001/popup",
                    },

                    PostLogoutRedirectUris = { "https://localhost:5001" },

                    AllowedScopes = { "openid", "profile", "EnergyPortalAPI" }
                }
            };
        }
    }
}