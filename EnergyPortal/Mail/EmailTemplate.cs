using System.Net;

namespace EnergyPortal.Mail
{
    /// <summary>
    /// Renders a <see cref="MailContent"/> as an HTML mail in EnergieZicht's colours (inline styles and tables,
    /// so mail clients show it as intended) and as a plain-text alternative.
    /// </summary>
    public static class EmailTemplate
    {
        private const string Brand = "#0f5257";
        private const string Ink = "#13201d";
        private const string InkMuted = "#4d5b57";
        private const string Surface = "#f3f5f4";
        private const string Font = "-apple-system, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif";

        public static string Html(MailContent mail)
        {
            static string E(string value) => WebUtility.HtmlEncode(value ?? string.Empty);

            return $"""
                <!DOCTYPE html>
                <html lang="nl">
                <head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1"><title>{E(mail.Subject)}</title></head>
                <body style="margin:0;padding:0;background:{Surface};">
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:{Surface};">
                  <tr><td align="center" style="padding:32px 16px;">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;">
                      <tr><td style="padding:0 4px 16px;font-family:{Font};font-size:18px;font-weight:700;color:{Brand};">EnergieZicht</td></tr>
                      <tr><td style="background:#ffffff;border-radius:12px;padding:32px;font-family:{Font};color:{Ink};">
                        <h1 style="margin:0 0 12px;font-size:22px;line-height:1.3;font-weight:600;color:{Ink};">{E(mail.Heading)}</h1>
                        <p style="margin:0 0 24px;font-size:16px;line-height:1.5;">{E(mail.Text)}</p>
                        <table role="presentation" cellpadding="0" cellspacing="0"><tr>
                          <td style="border-radius:8px;background:{Brand};">
                            <a href="{E(mail.Link)}" style="display:inline-block;padding:12px 24px;font-family:{Font};font-size:16px;font-weight:600;color:#ffffff;text-decoration:none;border-radius:8px;">{E(mail.ButtonText)}</a>
                          </td>
                        </tr></table>
                        <p style="margin:24px 0 0;font-size:13px;line-height:1.5;color:{InkMuted};">Werkt de knop niet? Kopieer dan deze link naar je browser:<br><a href="{E(mail.Link)}" style="color:{Brand};word-break:break-all;">{E(mail.Link)}</a></p>
                      </td></tr>
                      <tr><td style="padding:16px 4px 0;font-family:{Font};font-size:13px;line-height:1.5;color:{InkMuted};">{E(mail.Note)}</td></tr>
                    </table>
                  </td></tr>
                </table>
                </body>
                </html>
                """;
        }

        public static string Text(MailContent mail) =>
            $"""
            {mail.Heading}

            {mail.Text}

            {mail.ButtonText}: {mail.Link}

            {mail.Note}

            EnergieZicht
            """;
    }
}
