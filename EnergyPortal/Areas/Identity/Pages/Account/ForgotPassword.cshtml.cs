using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DatabaseInterface.Entities;
using Microsoft.AspNetCore.Identity;
using EnergyPortal.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace EnergyPortal.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        public const string MailUnavailable = "We kunnen op dit moment geen e-mail versturen. Probeer het later opnieuw.";

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMailService _mail;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IMailService mail)
        {
            _userManager = userManager;
            _mail = mail;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vul je e-mailadres in.")]
            [EmailAddress(ErrorMessage = "Dit is geen geldig e-mailadres.")]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Checked before the user lookup, so this error says nothing about whether the account exists.
            if (!_mail.IsConfigured)
            {
                ModelState.AddModelError(string.Empty, MailUnavailable);
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code },
                protocol: Request.Scheme);

            var sent = await _mail.TrySendAsync(Input.Email, new MailContent(
                Subject: "Nieuw wachtwoord instellen",
                Heading: "Nieuw wachtwoord instellen",
                Text: "Je hebt gevraagd om een nieuw wachtwoord voor EnergieZicht. Met de knop hieronder stel je een nieuw wachtwoord in.",
                ButtonText: "Nieuw wachtwoord instellen",
                Link: callbackUrl));
            if (!sent)
            {
                ModelState.AddModelError(string.Empty, MailUnavailable);
                return Page();
            }

            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}
