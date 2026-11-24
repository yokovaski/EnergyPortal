namespace EnergyPortal.Mail
{
    /// <summary>
    /// SMTP account that sends the account mails (password reset, e-mail confirmation), bound from the
    /// "Smtp" configuration section (env: Smtp__Host, ...). For Gmail: smtp.gmail.com, port 587 and an app password.
    /// No host or password = no mail; the pages then tell the user that mail is unavailable.
    /// </summary>
    public class SmtpOptions
    {
        public const string Section = "Smtp";

        public string Host { get; set; }
        /// <summary>587 = STARTTLS, 465 = implicit TLS.</summary>
        public int Port { get; set; } = 587;
        public string User { get; set; }
        public string Password { get; set; }
        /// <summary>Sender address; defaults to <see cref="User"/>.</summary>
        public string From { get; set; }
        public string FromName { get; set; } = "EnergieZicht";

        public string SenderAddress => string.IsNullOrWhiteSpace(From) ? User : From;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Host)
            && !string.IsNullOrEmpty(Password)
            && !string.IsNullOrWhiteSpace(SenderAddress);
    }
}
