using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class SeedRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "id", "name", "short_name" },
                values: new object[,]
                {
                    { 0, "Administrator", "Admin" },
                    { 1, "Staff", "Staff" },
                    { 2, "Store Owner", "Store" },
                    { 4, "Baker", "Baker" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "id",
                keyValue: 0);

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "id",
                keyValue: 4);
        }
    }
}
