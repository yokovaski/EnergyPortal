using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseInterface.Migrations
{
    public partial class AddedIndexToMetrics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TenSecondMetrics_Created",
                table: "TenSecondMetrics",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_MinuteMetrics_Created",
                table: "MinuteMetrics",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_HourMetrics_Created",
                table: "HourMetrics",
                column: "Created");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TenSecondMetrics_Created",
                table: "TenSecondMetrics");

            migrationBuilder.DropIndex(
                name: "IX_MinuteMetrics_Created",
                table: "MinuteMetrics");

            migrationBuilder.DropIndex(
                name: "IX_HourMetrics_Created",
                table: "HourMetrics");
        }
    }
}
