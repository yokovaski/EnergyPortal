using Microsoft.AspNetCore.Identity;

namespace DatabaseInterface.Entities
{
    public class ApplicationUser : IdentityUser
    {
        /// <summary>Display name; the app shows its initials instead of the email address.</summary>
        public string Name { get; set; }

        public virtual RaspberryPi RaspberryPi { get; set; }
        public virtual Settings Settings { get; set; }
    }
}