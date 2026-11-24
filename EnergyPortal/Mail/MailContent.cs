namespace EnergyPortal.Mail
{
    /// <summary>
    /// One account mail: a heading, a short text and a single action link, rendered by <see cref="EmailTemplate"/>.
    /// All values are plain text; the template encodes them.
    /// </summary>
    public sealed record MailContent(
        string Subject,
        string Heading,
        string Text,
        string ButtonText,
        string Link,
        string Note = "Heb je dit niet aangevraagd? Dan kun je deze e-mail negeren.");
}
