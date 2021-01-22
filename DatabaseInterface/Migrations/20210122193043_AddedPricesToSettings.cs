using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseInterface.Migrations
{
    public partial class AddedPricesToSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ElectricityDeliveryPricePerMonth",
                table: "Settings",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GasDeliveryPricePerMonth",
                table: "Settings",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GasPrice",
                table: "Settings",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HighRedeliveryPricePerKwh",
                table: "Settings",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HighUsagePricePerKwh",
                table: "Settings",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LowRedeliveryPricePerKwh",
                table: "Settings",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LowUsagePricePerKwh",
                table: "Settings",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElectricityDeliveryPricePerMonth",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "GasDeliveryPricePerMonth",
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
