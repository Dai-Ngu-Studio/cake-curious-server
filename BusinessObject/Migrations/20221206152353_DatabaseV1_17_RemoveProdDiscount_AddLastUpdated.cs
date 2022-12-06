using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class DatabaseV1_17_RemoveProdDiscount_AddLastUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "discount",
                table: "Product");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_updated",
                table: "Product",
                type: "datetime2(7)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_updated",
                table: "Product");

            migrationBuilder.AddColumn<decimal>(
                name: "discount",
                table: "Product",
                type: "decimal(20,4)",
                nullable: true);
        }
    }
}
