using System.Linq;
using System.Security.Claims;
using IdentityModel;

namespace EnergyPortal.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.FirstOrDefault(x => x.Type.Equals(JwtClaimTypes.Subject))?.Value;
        }
    }
}