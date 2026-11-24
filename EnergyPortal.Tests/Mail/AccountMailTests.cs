using System.Net;
using System.Text.RegularExpressions;
using DatabaseInterface.Entities;
using EnergyPortal.Areas.Identity.Pages.Account;
using EnergyPortal.Mail;
using EnergyPortal.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnergyPortal.Tests.Mail;

/// <summary>The Identity pages that send mail: "Wachtwoord vergeten" and the e-mail section of the Account page.</summary>
public class AccountMailTests : IAsyncLifetime
{
    private const string Confirmed = "confirmed";
    private const string Unconfirmed = "unconfirmed";

    private readonly FakeMailService mail = new();
    private readonly PortalFactory factory;

    public AccountMailTests()
    {
        factory = new PortalFactory
        {
            ConfigureServices = services =>
            {
                services.RemoveAll<IMailService>();
                services.AddSingleton<IMailService>(mail);
            }
        };
    }

    public async Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await users.CreateAsync(new ApplicationUser { Id = Confirmed, UserName = "confirmed@test.nl", Email = "confirmed@test.nl", EmailConfirmed = true });
        await users.CreateAsync(new ApplicationUser { Id = Unconfirmed, UserName = "unconfirmed@test.nl", Email = "unconfirmed@test.nl" });
    }

    public async Task DisposeAsync() => await factory.DisposeAsync();

    private static async Task<HttpResponseMessage> PostForm(HttpClient client, string page, string handler, Dictionary<string, string> fields)
    {
        var form = await client.GetStringAsync(page);
        var token = Regex.Match(form, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token);
        fields["__RequestVerificationToken"] = token;
        var url = handler == null ? page : $"{page}?handler={handler}";
        return await client.PostAsync(url, new FormUrlEncodedContent(fields));
    }

    private Task<HttpResponseMessage> ForgotPassword(string email) =>
        PostForm(factory.ClientFor(null), "/Identity/Account/ForgotPassword", null, new() { ["Input.Email"] = email });

    [Fact]
    public async Task Forgot_password_shows_an_error_when_mail_is_not_configured()
    {
        mail.Configured = false;

        var response = await ForgotPassword("confirmed@test.nl");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(WebUtility.HtmlEncode(ForgotPasswordModel.MailUnavailable), await response.Content.ReadAsStringAsync());
        Assert.Empty(mail.Sent);
    }

    [Fact]
    public async Task Forgot_password_shows_the_same_error_for_unknown_addresses_when_mail_is_not_configured()
    {
        mail.Configured = false;

        var response = await ForgotPassword("nobody@test.nl");

        Assert.Contains(WebUtility.HtmlEncode(ForgotPasswordModel.MailUnavailable), await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Forgot_password_mails_a_reset_link_to_a_confirmed_address()
    {
        var response = await ForgotPassword("confirmed@test.nl");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.EndsWith("/Identity/Account/ForgotPasswordConfirmation", response.Headers.Location!.ToString());
        var (to, content) = Assert.Single(mail.Sent);
        Assert.Equal("confirmed@test.nl", to);
        Assert.Contains("/Identity/Account/ResetPassword?code=", content.Link);
    }

    [Fact]
    public async Task Forgot_password_sends_nothing_for_unknown_addresses_but_redirects_the_same()
    {
        var response = await ForgotPassword("nobody@test.nl");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Empty(mail.Sent);
    }

    [Fact]
    public async Task Forgot_password_shows_an_error_when_sending_fails()
    {
        mail.Fail = true;

        var response = await ForgotPassword("confirmed@test.nl");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(WebUtility.HtmlEncode(ForgotPasswordModel.MailUnavailable), await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Account_page_offers_a_confirmation_mail_only_for_an_unconfirmed_address()
    {
        Assert.Contains("Stuur bevestigingsmail", await factory.ClientFor(Unconfirmed).GetStringAsync("/Identity/Account/Manage"));
        Assert.DoesNotContain("Stuur bevestigingsmail", await factory.ClientFor(Confirmed).GetStringAsync("/Identity/Account/Manage"));
    }

    [Fact]
    public async Task Confirmation_link_from_the_mail_confirms_the_address()
    {
        var client = factory.ClientFor(Unconfirmed);
        var response = await PostForm(client, "/Identity/Account/Manage", "SendConfirmation", new());

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var (to, content) = Assert.Single(mail.Sent);
        Assert.Equal("unconfirmed@test.nl", to);

        var confirm = await client.GetAsync(new Uri(content.Link).PathAndQuery);
        Assert.Contains("Je e-mailadres is bevestigd.", await confirm.Content.ReadAsStringAsync());
        Assert.True(await factory.QueryAsync(db => Task.FromResult(db.Users.Single(u => u.Id == Unconfirmed).EmailConfirmed)));
    }

    [Fact]
    public async Task Email_change_shows_an_error_instead_of_claiming_a_mail_was_sent_when_mail_is_not_configured()
    {
        mail.Configured = false;

        var response = await PostForm(factory.ClientFor(Confirmed), "/Identity/Account/Manage", "Email",
            new() { ["NewEmail.Email"] = "new@test.nl" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(WebUtility.HtmlEncode(ForgotPasswordModel.MailUnavailable), html);
        Assert.DoesNotContain("We hebben een bevestigingsmail gestuurd", html);
    }

    [Fact]
    public async Task Email_change_mails_a_confirmation_link_to_the_new_address()
    {
        var response = await PostForm(factory.ClientFor(Confirmed), "/Identity/Account/Manage", "Email",
            new() { ["NewEmail.Email"] = "new@test.nl" });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var (to, content) = Assert.Single(mail.Sent);
        Assert.Equal("new@test.nl", to);
        Assert.Contains("/Identity/Account/ConfirmEmailChange?", content.Link);
    }

    private sealed class FakeMailService : IMailService
    {
        public bool Configured { get; set; } = true;
        public bool Fail { get; set; }
        public List<(string To, MailContent Content)> Sent { get; } = [];

        public bool IsConfigured => Configured;

        public Task<bool> TrySendAsync(string to, MailContent content, CancellationToken ct = default)
        {
            if (!Configured || Fail)
                return Task.FromResult(false);
            Sent.Add((to, content));
            return Task.FromResult(true);
        }
    }
}
