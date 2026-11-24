using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseInterface.Migrations
{
    /// <summary>
    /// Data-only migration: marks the e-mail addresses of all users that exist at this point as confirmed.
    /// Until mail support was added nobody could confirm an address, and "Wachtwoord vergeten" only mails
    /// confirmed addresses. Users added or changing their address later confirm via the e-mail link.
    /// </summary>
    public partial class ConfirmExistingEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"AspNetUsers\" SET email_confirmed = TRUE WHERE email_confirmed = FALSE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible: which users were unconfirmed before is not recorded.
        }
    }
}
