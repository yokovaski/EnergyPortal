using DatabaseInterface;
using DatabaseInterface.Entities;
using DatabaseInterface.Repositories;
using EnergyPortal.Controllers.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EnergyPortal
{
    public class Startup
    {
        private const string ViteDevelopmentServer = "ViteDevelopmentServer";
        private readonly IHostEnvironment environment;

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            this.environment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            
            services.AddDatabaseDeveloperPageExceptionFilter();
            
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString = Configuration.GetConnectionString("DefaultConnection");
                options
                    .UseNpgsql(connectionString,
                        assembly => assembly.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                    .UseSnakeCaseNamingConvention();
            });

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();
            services.AddRazorPages();
            
            services.AddScoped<DbTenSecondMetricRepository, DbTenSecondMetricRepository>();
            services.AddScoped<DbMinuteMetricRepository, DbMinuteMetricRepository>();
            services.AddScoped<DbHourMetricRepository, DbHourMetricRepository>();
            services.AddScoped<DbSettingsRepository, DbSettingsRepository>();
            services.AddScoped<DbDeviceRepository, DbDeviceRepository>();

            // if (environment.IsDevelopment())
            // {
            //     // services.AddViteServices();
            //     services.AddCors(options =>
            //     {
            //         options.AddPolicy(ViteDevelopmentServer, builder =>
            //         {
            //             builder.WithOrigins("https://localhost:5173")
            //                 .AllowAnyHeader()
            //                 .AllowAnyMethod();
            //         });
            //     });
            // }
            
            services.AddCors(options =>
            {
                options.AddPolicy(ViteDevelopmentServer, builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                // app.UseViteDevMiddleware();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            // app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCors(ViteDevelopmentServer);

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.MapWhen(context => context.Request.Path.StartsWithSegments("/api/v3/energy"), builder =>
            {
                builder.UseAuthenticateByPiKey();
                app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}