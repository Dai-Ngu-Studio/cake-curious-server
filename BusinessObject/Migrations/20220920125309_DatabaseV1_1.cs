using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class DatabaseV1_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "latitude",
                table: "Store");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "Store");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "latitude",
                table: "Store",
                type: "decimal(12,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "longitude",
                table: "Store",
                type: "decimal(12,8)",
                nullable: true);
        }
    }
}
