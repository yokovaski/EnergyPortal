using Microsoft.AspNetCore.Builder;

namespace EnergyPortal.Controllers.Middleware
{
    public static class AuthenticateByPiKeyExtension
    {
        /// <summary>
        /// Register <see cref="AuthenticateByPiKey"/> to the given <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">Instance of <see cref="IApplicationBuilder"/>.</param>
        /// <returns><see cref="IApplicationBuilder"/> where <see cref="AuthenticateByPiKey"/> is added to</returns>
        public static IApplicationBuilder UseAuthenticateByPiKey(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthenticateByPiKey>();
        }
    }
}