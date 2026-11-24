using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EnergyPortal.Mail
{
    /// <summary>SMTP with STARTTLS (port 587) or implicit TLS (465), e.g. Gmail with an app password.</summary>
    public sealed class SmtpMailService(IOptions<SmtpOptions> options, ILogger<SmtpMailService> logger) : IMailService
    {
        private readonly SmtpOptions smtp = options.Value;

        public bool IsConfigured => smtp.IsConfigured;

        public async Task<bool> TrySendAsync(string to, MailContent mail, CancellationToken ct = default)
        {
            if (!smtp.IsConfigured)
            {
                logger.LogWarning("E-mail '{Subject}' niet verstuurd: SMTP is niet ingesteld (Smtp__Host, Smtp__User, Smtp__Password).", mail.Subject);
                return false;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtp.FromName, smtp.SenderAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = mail.Subject;
            message.Body = new BodyBuilder
            {
                TextBody = EmailTemplate.Text(mail),
                HtmlBody = EmailTemplate.Html(mail),
            }.ToMessageBody();

            try
            {
                using var client = new SmtpClient { Timeout = 30_000 };
                var tls = smtp.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
                await client.ConnectAsync(smtp.Host, smtp.Port, tls, ct);
                await client.AuthenticateAsync(string.IsNullOrWhiteSpace(smtp.User) ? smtp.SenderAddress : smtp.User, smtp.Password, ct);
                await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);
                logger.LogInformation("E-mail '{Subject}' verstuurd.", mail.Subject);
                return true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "E-mail '{Subject}' versturen via {Host}:{Port} mislukt.", mail.Subject, smtp.Host, smtp.Port);
                return false;
            }
        }
    }
}
