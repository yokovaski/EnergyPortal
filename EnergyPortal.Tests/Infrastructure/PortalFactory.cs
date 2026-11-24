using System.Security.Claims;
using System.Text.Encodings.Web;
using DatabaseInterface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnergyPortal.Tests.Infrastructure;

/// <summary>
/// Hosts the real Startup pipeline with an in-memory database and a header-driven auth scheme.
/// </summary>
public class PortalFactory : WebApplicationFactory<Startup>
{
    public const string UserHeader = "X-Test-UserId";
    private readonly string databaseName = Guid.NewGuid().ToString();

    /// <summary>Extra service registrations (applied last), e.g. a fake mail service.</summary>
    public Action<IServiceCollection> ConfigureServices { get; init; }

    // Program.Main runs migrations against Postgres; build the host from Startup directly instead.
    protected override IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(web => web.UseStartup<Startup>());

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(databaseName));

            services.AddDataProtection().UseEphemeralDataProtectionProvider();

            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, HeaderAuthHandler>(HeaderAuthHandler.SchemeName, _ => { });
            services.PostConfigure<AuthenticationOptions>(o =>
            {
                o.DefaultScheme = HeaderAuthHandler.SchemeName;
                o.DefaultAuthenticateScheme = HeaderAuthHandler.SchemeName;
                o.DefaultChallengeScheme = HeaderAuthHandler.SchemeName;
            });

            ConfigureServices?.Invoke(services);
        });
    }

    public async Task SeedAsync(Func<ApplicationDbContext, Task> seed)
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await seed(db);
        await db.SaveChangesAsync();
    }

    public async Task<T> QueryAsync<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        using var scope = Services.CreateScope();
        return await query(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    public HttpClient ClientFor(string userId)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        if (userId != null)
            client.DefaultRequestHeaders.Add(UserHeader, userId);
        return client;
    }
}

public class HeaderAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(PortalFactory.UserHeader, out var userId))
            return Task.FromResult(AuthenticateResult.NoResult());

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId!), new Claim(ClaimTypes.Name, $"{userId}@test")],
            SchemeName);
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}
