using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseInterface.Migrations
{
    public partial class AddedShowDayNameToSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowDayName",
                table: "Settings",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowDayName",
                table: "Settings");
        }
    }
}
