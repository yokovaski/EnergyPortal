using System.Linq;
using System.Threading.Tasks;
using DatabaseInterface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EnergyPortal.Controllers.Middleware
{
    public class AuthenticateByPiKey
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// Create instance of <see cref="AuthenticateByPiKey"/>.
        /// </summary>
        /// <param name="next">The next delegate/middleware in the pipeline.</param>
        public AuthenticateByPiKey(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Invoke the <see cref="AuthenticateByPiKey"/>.
        /// </summary>
        /// <param name="context">Current <see cref="HttpContext"/>.</param>
        /// <param name="dbContext">Instance of <see cref="ApplicationDbContext"/>.</param>
        /// <returns>
        /// Task that allow the pipeline to continue if the raspberry pi key could be found.
        /// </returns>
        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
            string rpiKey = context.Request.Headers["rpiKey"];

            var rpi = await dbContext.RaspberryPis.Where(r => r.RpiKey.Equals(rpiKey)).FirstOrDefaultAsync();

            if (rpi == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            // Call the next delegate/middleware in the pipeline
            await next(context);
        }
    }
}