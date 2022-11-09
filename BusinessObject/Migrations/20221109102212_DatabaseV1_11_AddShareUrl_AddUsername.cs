using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class DatabaseV1_11_AddShareUrl_AddUsername : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "share_url",
                table: "User",
                type: "nvarchar(2048)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "username",
                table: "User",
                type: "nvarchar(128)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "share_url",
                table: "Store",
                type: "nvarchar(2048)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "share_url",
                table: "Recipe",
                type: "nvarchar(2048)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "share_url",
                table: "Product",
                type: "nvarchar(2048)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_username",
                table: "User",
                column: "username",
                unique: true,
                filter: "[username] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_username",
                table: "User");

            migrationBuilder.DropColumn(
                name: "share_url",
                table: "User");

            migrationBuilder.DropColumn(
                name: "username",
                table: "User");

            migrationBuilder.DropColumn(
                name: "share_url",
                table: "Store");

            migrationBuilder.DropColumn(
                name: "share_url",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "share_url",
                table: "Product");
        }
    }
}
