using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EnergyPortal.Controllers.Middleware
{
    /// <summary>
    /// This middleware class redirects users that are not authenticated to the login page.
    ///
    /// This middleware is created for blocking access to the docs, which are served from wwwroot that, by design, is
    /// exposed to the whole world.
    /// </summary>
    public class OnlyPassAuthenticatedUsers
    {
        private readonly RequestDelegate next;
        
        public OnlyPassAuthenticatedUsers(RequestDelegate next)
        {
            this.next = next;
        }
        
        public Task Invoke(HttpContext context)
        {
            if (context?.User.Identity?.IsAuthenticated ?? false)
                return next(context);

            var pathString = context.Request.PathBase + context.Request.Path;
            var redirectTo = pathString.ToString().Remove(0, 1);
            
            context.Response.Redirect($"/Identity/Account/Login?ReturnUrl=%2F{redirectTo}");
            return Task.CompletedTask;
        }
    }
    
    public static class OnlyPassAuthenticatedUsersExtensions
    {
        public static IApplicationBuilder UseOnlyPassAuthenticatedUsers(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OnlyPassAuthenticatedUsers>();
        }
    }
}