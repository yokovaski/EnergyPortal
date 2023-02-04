using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseInterface.Migrations
{
    /// <inheritdoc />
    public partial class OpenIdDict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OpenIddictApplications",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    clientid = table.Column<string>(name: "client_id", type: "character varying(100)", maxLength: 100, nullable: true),
                    clientsecret = table.Column<string>(name: "client_secret", type: "text", nullable: true),
                    concurrencytoken = table.Column<string>(name: "concurrency_token", type: "character varying(50)", maxLength: 50, nullable: true),
                    consenttype = table.Column<string>(name: "consent_type", type: "character varying(50)", maxLength: 50, nullable: true),
                    displayname = table.Column<string>(name: "display_name", type: "text", nullable: true),
                    displaynames = table.Column<string>(name: "display_names", type: "text", nullable: true),
                    permissions = table.Column<string>(type: "text", nullable: true),
                    postlogoutredirecturis = table.Column<string>(name: "post_logout_redirect_uris", type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redirecturis = table.Column<string>(name: "redirect_uris", type: "text", nullable: true),
                    requirements = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictScopes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    concurrencytoken = table.Column<string>(name: "concurrency_token", type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    descriptions = table.Column<string>(type: "text", nullable: true),
                    displayname = table.Column<string>(name: "display_name", type: "text", nullable: true),
                    displaynames = table.Column<string>(name: "display_names", type: "text", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    resources = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_scopes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorizations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    applicationid = table.Column<string>(name: "application_id", type: "text", nullable: true),
                    concurrencytoken = table.Column<string>(name: "concurrency_token", type: "character varying(50)", maxLength: 50, nullable: true),
                    creationdate = table.Column<DateTime>(name: "creation_date", type: "timestamp with time zone", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    scopes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_authorizations", x => x.id);
                    table.ForeignKey(
                        name: "fk_open_iddict_authorizations_open_iddict_applications_application",
                        column: x => x.applicationid,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    applicationid = table.Column<string>(name: "application_id", type: "text", nullable: true),
                    authorizationid = table.Column<string>(name: "authorization_id", type: "text", nullable: true),
                    concurrencytoken = table.Column<string>(name: "concurrency_token", type: "character varying(50)", maxLength: 50, nullable: true),
                    creationdate = table.Column<DateTime>(name: "creation_date", type: "timestamp with time zone", nullable: true),
                    expirationdate = table.Column<DateTime>(name: "expiration_date", type: "timestamp with time zone", nullable: true),
                    payload = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redemptiondate = table.Column<DateTime>(name: "redemption_date", type: "timestamp with time zone", nullable: true),
                    referenceid = table.Column<string>(name: "reference_id", type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                        column: x => x.applicationid,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                        column: x => x.authorizationid,
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_applications_client_id",
                table: "OpenIddictApplications",
                column: "client_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_authorizations_application_id_status_subject_type",
                table: "OpenIddictAuthorizations",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_scopes_name",
                table: "OpenIddictScopes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_application_id_status_subject_type",
                table: "OpenIddictTokens",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_authorization_id",
                table: "OpenIddictTokens",
                column: "authorization_id");

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_reference_id",
                table: "OpenIddictTokens",
                column: "reference_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpenIddictScopes");

            migrationBuilder.DropTable(
                name: "OpenIddictTokens");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations");

            migrationBuilder.DropTable(
                name: "OpenIddictApplications");
        }
    }
}
