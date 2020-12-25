using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseInterface.Migrations
{
    public partial class AddedPricesToSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ElectricityDeliveryPricePerYear",
                table: "Settings",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "GasDeliveryPricePerYear",
                table: "Settings",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "GasPrice",
                table: "Settings",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HighRedeliveryPricePerKwh",
                table: "Settings",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "HighUsagePricePerKwh",
                table: "Settings",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LowRedeliveryPricePerKwh",
                table: "Settings",
                type: "double",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LowUsagePricePerKwh",
                table: "Settings",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElectricityDeliveryPricePerYear",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "GasDeliveryPricePerYear",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "GasPrice",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "HighRedeliveryPricePerKwh",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "HighUsagePricePerKwh",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "LowRedeliveryPricePerKwh",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "LowUsagePricePerKwh",
                table: "Settings");
        }
    }
}
