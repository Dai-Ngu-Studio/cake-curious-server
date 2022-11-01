using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class DatabaseV1_10_AddCreatedDateUserStore_RemoveColor_RemoveMeasurement : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Color");

            migrationBuilder.DropTable(
                name: "Measurement");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_date",
                table: "User",
                type: "datetime2(7)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_date",
                table: "Store",
                type: "datetime2(7)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_date",
                table: "User");

            migrationBuilder.DropColumn(
                name: "created_date",
                table: "Store");

            migrationBuilder.CreateTable(
                name: "Color",
                columns: table => new
                {
                    hex_code = table.Column<string>(type: "varchar(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Color", x => x.hex_code);
                });

            migrationBuilder.CreateTable(
                name: "Measurement",
                columns: table => new
                {
                    measurement_unit = table.Column<string>(type: "nvarchar(24)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurement", x => x.measurement_unit);
                });
        }
    }
}
