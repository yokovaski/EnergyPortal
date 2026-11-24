using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using DatabaseInterface.Entities;
using EnergyPortal.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace EnergyPortal.Areas.Identity.Pages.Account.Manage
{
    /// <summary>
    /// The Account page: profile, e-mail, password, two-step verification and personal data on one page.
    /// Each form posts to its own handler and binds only its own input (prefix), so validation stays per section.
    /// </summary>
    public class IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IMailService mail,
        ILogger<IndexModel> logger) : PageModel
    {
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool Is2faEnabled { get; set; }
        public bool HasAuthenticator { get; set; }
        public int RecoveryCodesLeft { get; set; }
        public bool IsMachineRemembered { get; set; }

        /// <summary>Section the last post belonged to, for messages and errors.</summary>
        public string Section { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string StatusSection { get; set; }

        public ProfileInput Profile { get; set; } = new();
        public EmailInput NewEmail { get; set; } = new();
        public PasswordInput Password { get; set; } = new();

        public class ProfileInput
        {
            [StringLength(100, ErrorMessage = "Gebruik maximaal 100 tekens.")]
            public string Name { get; set; }

            [Phone(ErrorMessage = "Dit is geen geldig telefoonnummer.")]
            public string PhoneNumber { get; set; }
        }

        public class EmailInput
        {
            [Required(ErrorMessage = "Vul een e-mailadres in.")]
            [EmailAddress(ErrorMessage = "Dit is geen geldig e-mailadres.")]
            public string Email { get; set; }
        }

        public class PasswordInput
        {
            [Required(ErrorMessage = "Vul je huidige wachtwoord in.")]
            [DataType(DataType.Password)]
            public string OldPassword { get; set; }

            [Required(ErrorMessage = "Vul een nieuw wachtwoord in.")]
            [StringLength(100, ErrorMessage = "Het wachtwoord moet minimaal {2} tekens hebben.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Compare(nameof(NewPassword), ErrorMessage = "De wachtwoorden zijn niet gelijk.")]
            public string ConfirmPassword { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user, bool fillProfile = true)
        {
            Email = await userManager.GetEmailAsync(user);
            EmailConfirmed = await userManager.IsEmailConfirmedAsync(user);
            Is2faEnabled = await userManager.GetTwoFactorEnabledAsync(user);
            HasAuthenticator = await userManager.GetAuthenticatorKeyAsync(user) != null;
            RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);
            IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user);
            if (fillProfile)
                Profile = new ProfileInput { Name = user.Name, PhoneNumber = await userManager.GetPhoneNumberAsync(user) };
        }

        private IActionResult Done(string section, string message)
        {
            StatusMessage = message;
            StatusSection = section;
            return RedirectToPage("./Index", null, section);
        }

        private async Task<IActionResult> Invalid(ApplicationUser user, string section, bool fillProfile = true)
        {
            Section = section;
            await LoadAsync(user, fillProfile);
            return Page();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Gebruiker niet gevonden.");

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostProfileAsync([Bind(Prefix = nameof(Profile))] ProfileInput input)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Gebruiker niet gevonden.");

            Profile = input;
            if (!ModelState.IsValid)
                return await Invalid(user, "profiel", fillProfile: false);

            user.Name = string.IsNullOrWhiteSpace(input.Name) ? null : input.Name.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(input.PhoneNumber) ? null : input.PhoneNumber.Trim();
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return await Invalid(user, "profiel", fillProfile: false);
            }

            await signInManager.RefreshSignInAsync(user);
            return Done("profiel", "Je profiel is opgeslagen.");
        }

        public async Task<IActionResult> OnPostEmailAsync([Bind(Prefix = nameof(NewEmail))] EmailInput input)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Gebruiker niet gevonden.");

            NewEmail = input;
            if (!ModelState.IsValid)
                return await Invalid(user, "e-mailadres");

            var email = await userManager.GetEmailAsync(user);
            if (input.Email == email)
                return Done("e-mailadres", "Dit is al je e-mailadres.");

            if (!mail.IsConfigured)
                return await MailUnavailable(user);

            var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateChangeEmailTokenAsync(user, input.Email);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page("/Account/ConfirmEmailChange", pageHandler: null,
                values: new { area = "Identity", userId, email = input.Email, code }, protocol: Request.Scheme);
            var sent = await mail.TrySendAsync(input.Email, new MailContent(
                Subject: "Bevestig je nieuwe e-mailadres",
                Heading: "Bevestig je nieuwe e-mailadres",
                Text: "Je wilt dit adres gebruiken om in te loggen bij EnergieZicht. Bevestig het met de knop hieronder; daarna log je in met dit adres.",
                ButtonText: "E-mailadres bevestigen",
                Link: callbackUrl));
            if (!sent)
                return await MailUnavailable(user);

            return Done("e-mailadres", $"We hebben een bevestigingsmail gestuurd naar {input.Email}.");
        }

        /// <summary>Sends a confirmation link for the current (still unconfirmed) address.</summary>
        public async Task<IActionResult> OnPostSendConfirmationAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Gebruiker niet gevonden.");

            if (await userManager.IsEmailConfirmedAsync(user))
                return Done("e-mailadres", "Je e-mailadres is al bevestigd.");

            if (!mail.IsConfigured)
                return await MailUnavailable(user);

            var email = await userManager.GetEmailAsync(user);
            var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page("/Account/ConfirmEmail", pageHandler: null,
                values: new { area = "Identity", userId, code }, protocol: Request.Scheme);
            var sent = await mail.TrySendAsync(email, new MailContent(
                Subject: "Bevestig je e-mailadres",
                Heading: "Bevestig je e-mailadres",
                Text: "Bevestig je e-mailadres voor EnergieZicht met de knop hieronder. Daarna kun je ook een nieuw wachtwoord aanvragen als je het vergeten bent.",
                ButtonText: "E-mailadres bevestigen",
                Link: callbackUrl));
            if (!sent)
                return await MailUnavailable(user);

            return Done("e-mailadres", $"We hebben een bevestigingsmail gestuurd naar {email}.");
        }

        private async Task<IActionResult> MailUnavailable(ApplicationUser user)
        {
            ModelState.AddModelError(string.Empty, ForgotPasswordModel.MailUnavailable);
            return await Invalid(user, "e-mailadres");
        }

        public async Task<IActionResult> OnPostPasswordAsync([Bind(Prefix = nameof(Password))] PasswordInput input)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Gebruiker niet gevonden.");

            if (!ModelState.IsValid)
                return await Invalid(user, "wachtwoord");

            var result = await userManager.ChangePasswordAsync(user, input.OldPassword, input.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
                return await Invalid(user, "wachtwoord");
            }

            await signInManager.RefreshSignInAsync(user);
            logger.LogInformation("User changed their password successfully.");
            return Done("wachtwoord", "Je wachtwoord is gewijzigd.");
        }

        public async Task<IActionResult> OnPostForgetBrowserAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return NotFound("Gebruiker niet gevonden.");

            await signInManager.ForgetTwoFactorClientAsync();
            return Done("tweestapsverificatie", "Dit apparaat is vergeten. Bij de volgende keer inloggen vragen we weer om een code.");
        }
    }
}
