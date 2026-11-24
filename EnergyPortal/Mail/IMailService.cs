using System.Threading;
using System.Threading.Tasks;

namespace EnergyPortal.Mail
{
    /// <summary>Sends account mails. Pages check <see cref="IsConfigured"/> first and show an error when it is false.</summary>
    public interface IMailService
    {
        bool IsConfigured { get; }

        /// <summary>
        /// Sends <paramref name="mail"/> to <paramref name="to"/>. Returns false (and logs why) when SMTP is not
        /// configured or sending failed, so callers can show an error instead of claiming a mail was sent.
        /// </summary>
        Task<bool> TrySendAsync(string to, MailContent mail, CancellationToken ct = default);
    }
}
