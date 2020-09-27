using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseInterface.Migrations
{
    public partial class RemovedDayMetric : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DayMetrics");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DayMetrics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    RaspberryPiId = table.Column<long>(type: "bigint", nullable: false),
                    RedeliveryNow = table.Column<int>(type: "int", nullable: false),
                    RedeliveryTotalHigh = table.Column<long>(type: "bigint", nullable: false),
                    RedeliveryTotalLow = table.Column<long>(type: "bigint", nullable: false),
                    SolarNow = table.Column<int>(type: "int", nullable: false),
                    SolarTotal = table.Column<long>(type: "bigint", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UsageGasNow = table.Column<int>(type: "int", nullable: false),
                    UsageGasTotal = table.Column<long>(type: "bigint", nullable: false),
                    UsageNow = table.Column<int>(type: "int", nullable: false),
                    UsageTotalHigh = table.Column<long>(type: "bigint", nullable: false),
                    UsageTotalLow = table.Column<long>(type: "bigint", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_DayMetrics_RaspberryPiId",
                table: "DayMetrics",
                column: "RaspberryPiId");
        }
    }
}
