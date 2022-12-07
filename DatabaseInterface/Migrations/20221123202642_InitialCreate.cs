using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DatabaseInterface.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalizedname = table.Column<string>(name: "normalized_name", type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrencystamp = table.Column<string>(name: "concurrency_stamp", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    username = table.Column<string>(name: "user_name", type: "character varying(256)", maxLength: 256, nullable: true),
                    normalizedusername = table.Column<string>(name: "normalized_user_name", type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalizedemail = table.Column<string>(name: "normalized_email", type: "character varying(256)", maxLength: 256, nullable: true),
                    emailconfirmed = table.Column<bool>(name: "email_confirmed", type: "boolean", nullable: false),
                    passwordhash = table.Column<string>(name: "password_hash", type: "text", nullable: true),
                    securitystamp = table.Column<string>(name: "security_stamp", type: "text", nullable: true),
                    concurrencystamp = table.Column<string>(name: "concurrency_stamp", type: "text", nullable: true),
                    phonenumber = table.Column<string>(name: "phone_number", type: "text", nullable: true),
                    phonenumberconfirmed = table.Column<bool>(name: "phone_number_confirmed", type: "boolean", nullable: false),
                    twofactorenabled = table.Column<bool>(name: "two_factor_enabled", type: "boolean", nullable: false),
                    lockoutend = table.Column<DateTimeOffset>(name: "lockout_end", type: "timestamp with time zone", nullable: true),
                    lockoutenabled = table.Column<bool>(name: "lockout_enabled", type: "boolean", nullable: false),
                    accessfailedcount = table.Column<int>(name: "access_failed_count", type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    roleid = table.Column<string>(name: "role_id", type: "text", nullable: false),
                    claimtype = table.Column<string>(name: "claim_type", type: "text", nullable: true),
                    claimvalue = table.Column<string>(name: "claim_value", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.roleid,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<string>(name: "user_id", type: "text", nullable: false),
                    claimtype = table.Column<string>(name: "claim_type", type: "text", nullable: true),
                    claimvalue = table.Column<string>(name: "claim_value", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claims_asp_net_users_user_id",
                        column: x => x.userid,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    loginprovider = table.Column<string>(name: "login_provider", type: "character varying(128)", maxLength: 128, nullable: false),
                    providerkey = table.Column<string>(name: "provider_key", type: "character varying(128)", maxLength: 128, nullable: false),
                    providerdisplayname = table.Column<string>(name: "provider_display_name", type: "text", nullable: true),
                    userid = table.Column<string>(name: "user_id", type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_logins", x => new { x.loginprovider, x.providerkey });
                    table.ForeignKey(
                        name: "fk_asp_net_user_logins_asp_net_users_user_id",
                        column: x => x.userid,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    userid = table.Column<string>(name: "user_id", type: "text", nullable: false),
                    roleid = table.Column<string>(name: "role_id", type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_roles", x => new { x.userid, x.roleid });
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.roleid,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.userid,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    userid = table.Column<string>(name: "user_id", type: "text", nullable: false),
                    loginprovider = table.Column<string>(name: "login_provider", type: "character varying(128)", maxLength: 128, nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_tokens", x => new { x.userid, x.loginprovider, x.name });
                    table.ForeignKey(
                        name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                        column: x => x.userid,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "raspberry_pis",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    rpikey = table.Column<string>(name: "rpi_key", type: "text", nullable: true),
                    userid = table.Column<string>(name: "user_id", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_raspberry_pis", x => x.id);
                    table.ForeignKey(
                        name: "fk_raspberry_pis_users_user_id",
                        column: x => x.userid,
                        principalTable: "AspNetUsers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timezoneid = table.Column<string>(name: "time_zone_id", type: "text", nullable: true),
                    solarsystem = table.Column<bool>(name: "solar_system", type: "boolean", nullable: false),
                    showdayname = table.Column<bool>(name: "show_day_name", type: "boolean", nullable: false),
                    userid = table.Column<string>(name: "user_id", type: "text", nullable: true),
                    gasprice = table.Column<decimal>(name: "gas_price", type: "numeric", nullable: false),
                    highusagepriceperkwh = table.Column<decimal>(name: "high_usage_price_per_kwh", type: "numeric", nullable: false),
                    lowusagepriceperkwh = table.Column<decimal>(name: "low_usage_price_per_kwh", type: "numeric", nullable: false),
                    highredeliverypriceperkwh = table.Column<decimal>(name: "high_redelivery_price_per_kwh", type: "numeric", nullable: false),
                    lowredeliverypriceperkwh = table.Column<decimal>(name: "low_redelivery_price_per_kwh", type: "numeric", nullable: false),
                    electricitydeliverypricepermonth = table.Column<decimal>(name: "electricity_delivery_price_per_month", type: "numeric", nullable: false),
                    gasdeliverypricepermonth = table.Column<decimal>(name: "gas_delivery_price_per_month", type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_settings", x => x.id);
                    table.ForeignKey(
                        name: "fk_settings_users_user_id",
                        column: x => x.userid,
                        principalTable: "AspNetUsers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "hour_metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    raspberrypiid = table.Column<long>(name: "raspberry_pi_id", type: "bigint", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    usagenow = table.Column<int>(name: "usage_now", type: "integer", nullable: false),
                    redeliverynow = table.Column<int>(name: "redelivery_now", type: "integer", nullable: false),
                    solarnow = table.Column<int>(name: "solar_now", type: "integer", nullable: false),
                    usagetotalhigh = table.Column<long>(name: "usage_total_high", type: "bigint", nullable: false),
                    redeliverytotalhigh = table.Column<long>(name: "redelivery_total_high", type: "bigint", nullable: false),
                    usagetotallow = table.Column<long>(name: "usage_total_low", type: "bigint", nullable: false),
                    redeliverytotallow = table.Column<long>(name: "redelivery_total_low", type: "bigint", nullable: false),
                    solartotal = table.Column<long>(name: "solar_total", type: "bigint", nullable: false),
                    usagegasnow = table.Column<int>(name: "usage_gas_now", type: "integer", nullable: false),
                    usagegastotal = table.Column<long>(name: "usage_gas_total", type: "bigint", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hour_metrics", x => x.id);
                    table.ForeignKey(
                        name: "fk_hour_metrics_raspberry_pis_raspberry_pi_id",
                        column: x => x.raspberrypiid,
                        principalTable: "raspberry_pis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "minute_metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    raspberrypiid = table.Column<long>(name: "raspberry_pi_id", type: "bigint", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    usagenow = table.Column<int>(name: "usage_now", type: "integer", nullable: false),
                    redeliverynow = table.Column<int>(name: "redelivery_now", type: "integer", nullable: false),
                    solarnow = table.Column<int>(name: "solar_now", type: "integer", nullable: false),
                    usagetotalhigh = table.Column<long>(name: "usage_total_high", type: "bigint", nullable: false),
                    redeliverytotalhigh = table.Column<long>(name: "redelivery_total_high", type: "bigint", nullable: false),
                    usagetotallow = table.Column<long>(name: "usage_total_low", type: "bigint", nullable: false),
                    redeliverytotallow = table.Column<long>(name: "redelivery_total_low", type: "bigint", nullable: false),
                    solartotal = table.Column<long>(name: "solar_total", type: "bigint", nullable: false),
                    usagegasnow = table.Column<int>(name: "usage_gas_now", type: "integer", nullable: false),
                    usagegastotal = table.Column<long>(name: "usage_gas_total", type: "bigint", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_minute_metrics", x => x.id);
                    table.ForeignKey(
                        name: "fk_minute_metrics_raspberry_pis_raspberry_pi_id",
                        column: x => x.raspberrypiid,
                        principalTable: "raspberry_pis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ten_second_metrics",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    raspberrypiid = table.Column<long>(name: "raspberry_pi_id", type: "bigint", nullable: false),
                    mode = table.Column<int>(type: "integer", nullable: false),
                    usagenow = table.Column<int>(name: "usage_now", type: "integer", nullable: false),
                    redeliverynow = table.Column<int>(name: "redelivery_now", type: "integer", nullable: false),
                    solarnow = table.Column<int>(name: "solar_now", type: "integer", nullable: false),
                    usagetotalhigh = table.Column<long>(name: "usage_total_high", type: "bigint", nullable: false),
                    redeliverytotalhigh = table.Column<long>(name: "redelivery_total_high", type: "bigint", nullable: false),
                    usagetotallow = table.Column<long>(name: "usage_total_low", type: "bigint", nullable: false),
                    redeliverytotallow = table.Column<long>(name: "redelivery_total_low", type: "bigint", nullable: false),
                    solartotal = table.Column<long>(name: "solar_total", type: "bigint", nullable: false),
                    usagegasnow = table.Column<int>(name: "usage_gas_now", type: "integer", nullable: false),
                    usagegastotal = table.Column<long>(name: "usage_gas_total", type: "bigint", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ten_second_metrics", x => x.id);
                    table.ForeignKey(
                        name: "fk_ten_second_metrics_raspberry_pis_raspberry_pi_id",
                        column: x => x.raspberrypiid,
                        principalTable: "raspberry_pis",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "AspNetRoleClaims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "AspNetUserClaims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "AspNetUserLogins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "AspNetUserRoles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hour_metrics_created",
                table: "hour_metrics",
                column: "created");

            migrationBuilder.CreateIndex(
                name: "ix_hour_metrics_raspberry_pi_id",
                table: "hour_metrics",
                column: "raspberry_pi_id");

            migrationBuilder.CreateIndex(
                name: "ix_minute_metrics_created",
                table: "minute_metrics",
                column: "created");

            migrationBuilder.CreateIndex(
                name: "ix_minute_metrics_raspberry_pi_id",
                table: "minute_metrics",
                column: "raspberry_pi_id");

            migrationBuilder.CreateIndex(
                name: "ix_raspberry_pis_user_id",
                table: "raspberry_pis",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_settings_user_id",
                table: "settings",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ten_second_metrics_created",
                table: "ten_second_metrics",
                column: "created");

            migrationBuilder.CreateIndex(
                name: "ix_ten_second_metrics_raspberry_pi_id",
                table: "ten_second_metrics",
                column: "raspberry_pi_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "hour_metrics");

            migrationBuilder.DropTable(
                name: "minute_metrics");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "ten_second_metrics");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "raspberry_pis");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
