using Microsoft.AspNetCore.Identity;

namespace EnergyPortal.Areas.Identity
{
    /// <summary>Dutch texts for the Identity errors a user can actually run into (password rules, e-mail, tokens).</summary>
    public class DutchIdentityErrorDescriber : IdentityErrorDescriber
    {
        private static IdentityError Error(string code, string description) => new() { Code = code, Description = description };

        public override IdentityError PasswordMismatch() => Error(nameof(PasswordMismatch), "Je huidige wachtwoord klopt niet.");
        public override IdentityError InvalidToken() => Error(nameof(InvalidToken), "De link is verlopen of al gebruikt. Vraag een nieuwe aan.");
        public override IdentityError InvalidEmail(string email) => Error(nameof(InvalidEmail), $"'{email}' is geen geldig e-mailadres.");
        public override IdentityError DuplicateEmail(string email) => Error(nameof(DuplicateEmail), $"'{email}' is al in gebruik.");
        public override IdentityError DuplicateUserName(string userName) => Error(nameof(DuplicateUserName), $"'{userName}' is al in gebruik.");
        public override IdentityError PasswordTooShort(int length) => Error(nameof(PasswordTooShort), $"Het wachtwoord moet minimaal {length} tekens hebben.");
        public override IdentityError PasswordRequiresDigit() => Error(nameof(PasswordRequiresDigit), "Het wachtwoord moet een cijfer bevatten.");
        public override IdentityError PasswordRequiresLower() => Error(nameof(PasswordRequiresLower), "Het wachtwoord moet een kleine letter bevatten.");
        public override IdentityError PasswordRequiresUpper() => Error(nameof(PasswordRequiresUpper), "Het wachtwoord moet een hoofdletter bevatten.");
        public override IdentityError PasswordRequiresNonAlphanumeric() => Error(nameof(PasswordRequiresNonAlphanumeric), "Het wachtwoord moet een leesteken bevatten, zoals ! of #.");
        public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Error(nameof(PasswordRequiresUniqueChars), $"Het wachtwoord moet minimaal {uniqueChars} verschillende tekens bevatten.");
    }
}
