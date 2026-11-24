using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseInterface.Migrations
{
    /// <inheritdoc />
    public partial class AddTemperature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "settings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "settings",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "postcode",
                table: "settings",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "temperatures",
                columns: table => new
                {
                    station_id = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_temperatures", x => new { x.station_id, x.created });
                });

            migrationBuilder.CreateTable(
                name: "weather_stations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_weather_stations", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "temperatures");

            migrationBuilder.DropTable(
                name: "weather_stations");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "settings");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "settings");

            migrationBuilder.DropColumn(
                name: "postcode",
                table: "settings");
        }
    }
}
