using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseInterface.Migrations
{
    public partial class InitialMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RaspberryPis",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: false),
                    RpiKey = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaspberryPis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RaspberryPis_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DayMetrics",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaspberryPiId = table.Column<long>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    UsageNow = table.Column<int>(nullable: false),
                    RedeliveryNow = table.Column<int>(nullable: false),
                    SolarNow = table.Column<int>(nullable: false),
                    UsageTotalHigh = table.Column<long>(nullable: false),
                    RedeliveryTotalHigh = table.Column<long>(nullable: false),
                    UsageTotalLow = table.Column<long>(nullable: false),
                    RedeliveryTotalLow = table.Column<long>(nullable: false),
                    SolarTotal = table.Column<long>(nullable: false),
                    UsageGasNow = table.Column<int>(nullable: false),
                    UsageGasTotal = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayMetrics_RaspberryPis_RaspberryPiId",
                        column: x => x.RaspberryPiId,
                        principalTable: "RaspberryPis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HourMetrics",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaspberryPiId = table.Column<long>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    UsageNow = table.Column<int>(nullable: false),
                    RedeliveryNow = table.Column<int>(nullable: false),
                    SolarNow = table.Column<int>(nullable: false),
                    UsageTotalHigh = table.Column<long>(nullable: false),
                    RedeliveryTotalHigh = table.Column<long>(nullable: false),
                    UsageTotalLow = table.Column<long>(nullable: false),
                    RedeliveryTotalLow = table.Column<long>(nullable: false),
                    SolarTotal = table.Column<long>(nullable: false),
                    UsageGasNow = table.Column<int>(nullable: false),
                    UsageGasTotal = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HourMetrics_RaspberryPis_RaspberryPiId",
                        column: x => x.RaspberryPiId,
                        principalTable: "RaspberryPis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MinuteMetrics",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaspberryPiId = table.Column<long>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    UsageNow = table.Column<int>(nullable: false),
                    RedeliveryNow = table.Column<int>(nullable: false),
                    SolarNow = table.Column<int>(nullable: false),
                    UsageTotalHigh = table.Column<long>(nullable: false),
                    RedeliveryTotalHigh = table.Column<long>(nullable: false),
                    UsageTotalLow = table.Column<long>(nullable: false),
                    RedeliveryTotalLow = table.Column<long>(nullable: false),
                    SolarTotal = table.Column<long>(nullable: false),
                    UsageGasNow = table.Column<int>(nullable: false),
                    UsageGasTotal = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinuteMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MinuteMetrics_RaspberryPis_RaspberryPiId",
                        column: x => x.RaspberryPiId,
                        principalTable: "RaspberryPis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenSecondMetrics",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RaspberryPiId = table.Column<long>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    UsageNow = table.Column<int>(nullable: false),
                    RedeliveryNow = table.Column<int>(nullable: false),
                    SolarNow = table.Column<int>(nullable: false),
                    UsageTotalHigh = table.Column<long>(nullable: false),
                    RedeliveryTotalHigh = table.Column<long>(nullable: false),
                    UsageTotalLow = table.Column<long>(nullable: false),
                    RedeliveryTotalLow = table.Column<long>(nullable: false),
                    SolarTotal = table.Column<long>(nullable: false),
                    UsageGasNow = table.Column<int>(nullable: false),
                    UsageGasTotal = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Updated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenSecondMetrics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenSecondMetrics_RaspberryPis_RaspberryPiId",
                        column: x => x.RaspberryPiId,
                        principalTable: "RaspberryPis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DayMetrics_RaspberryPiId",
                table: "DayMetrics",
                column: "RaspberryPiId");

            migrationBuilder.CreateIndex(
                name: "IX_HourMetrics_RaspberryPiId",
                table: "HourMetrics",
                column: "RaspberryPiId");

            migrationBuilder.CreateIndex(
                name: "IX_MinuteMetrics_RaspberryPiId",
                table: "MinuteMetrics",
                column: "RaspberryPiId");

            migrationBuilder.CreateIndex(
                name: "IX_RaspberryPis_UserId",
                table: "RaspberryPis",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenSecondMetrics_RaspberryPiId",
                table: "TenSecondMetrics",
                column: "RaspberryPiId");
        }

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
                name: "DayMetrics");

            migrationBuilder.DropTable(
                name: "HourMetrics");

            migrationBuilder.DropTable(
                name: "MinuteMetrics");

            migrationBuilder.DropTable(
                name: "TenSecondMetrics");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "RaspberryPis");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
