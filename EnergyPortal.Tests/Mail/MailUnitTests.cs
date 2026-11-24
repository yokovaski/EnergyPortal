using EnergyPortal.Mail;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EnergyPortal.Tests.Mail;

public class EmailTemplateTests
{
    private static readonly MailContent Mail = new(
        Subject: "Onderwerp", Heading: "Kop <b>", Text: "Tekst & meer", ButtonText: "Klik",
        Link: "https://example.test/reset?code=a&b=\"c\"");

    [Fact]
    public void Html_encodes_content_and_contains_the_link()
    {
        var html = EmailTemplate.Html(Mail);

        Assert.Contains("Kop &lt;b&gt;", html);
        Assert.Contains("Tekst &amp; meer", html);
        Assert.Contains("href=\"https://example.test/reset?code=a&amp;b=&quot;c&quot;\"", html);
        Assert.DoesNotContain("<b>", html);
        Assert.Contains("EnergieZicht", html);
    }

    [Fact]
    public void Text_contains_heading_text_and_raw_link()
    {
        var text = EmailTemplate.Text(Mail);

        Assert.Contains("Kop <b>", text);
        Assert.Contains("Tekst & meer", text);
        Assert.Contains("Klik: https://example.test/reset?code=a&b=\"c\"", text);
        Assert.Contains(Mail.Note, text);
    }
}

public class SmtpOptionsTests
{
    [Theory]
    [InlineData("smtp.gmail.com", "me@gmail.com", "secret", null, true)]
    [InlineData("smtp.gmail.com", null, "secret", "from@gmail.com", true)]
    [InlineData(null, "me@gmail.com", "secret", null, false)]
    [InlineData("smtp.gmail.com", "me@gmail.com", "", null, false)]
    [InlineData("smtp.gmail.com", null, "secret", null, false)]
    public void IsConfigured_requires_host_password_and_sender(string host, string user, string password, string from, bool expected)
    {
        var options = new SmtpOptions { Host = host, User = user, Password = password, From = from };
        Assert.Equal(expected, options.IsConfigured);
    }

    [Fact]
    public void Sender_defaults_to_user()
    {
        Assert.Equal("me@gmail.com", new SmtpOptions { User = "me@gmail.com" }.SenderAddress);
        Assert.Equal("alias@gmail.com", new SmtpOptions { User = "me@gmail.com", From = "alias@gmail.com" }.SenderAddress);
    }

    [Fact]
    public async Task Unconfigured_service_does_not_send()
    {
        var service = new SmtpMailService(Options.Create(new SmtpOptions()), NullLogger<SmtpMailService>.Instance);

        Assert.False(service.IsConfigured);
        Assert.False(await service.TrySendAsync("x@test", new MailContent("s", "h", "t", "b", "https://l")));
    }
}
