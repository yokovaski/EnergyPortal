using Microsoft.AspNetCore.Identity;

namespace DatabaseInterface.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public virtual RaspberryPi RaspberryPi { get; set; }
        public virtual Settings Settings { get; set; }
    }
}