using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class DatabaseV1_2_RemovePassword_SeedAdmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "password",
                table: "User");

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "id", "address", "citizenship_date", "citizenship_number", "date_of_birth", "display_name", "email", "full_name", "gender", "photo_url", "status", "store_id" },
                values: new object[] { "y0Bqpw0nQSaq4rJnZzntgmkQ6ar1", null, null, null, null, "Administrator", "admin@cakecurious.net", null, null, null, 0, null });

            migrationBuilder.InsertData(
                table: "UserHasRole",
                columns: new[] { "id", "role_id", "user_id" },
                values: new object[] { new Guid("248231d9-3f05-473d-9135-7be4188e0635"), 0, "y0Bqpw0nQSaq4rJnZzntgmkQ6ar1" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserHasRole",
                keyColumn: "id",
                keyValue: new Guid("248231d9-3f05-473d-9135-7be4188e0635"));

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "id",
                keyValue: "y0Bqpw0nQSaq4rJnZzntgmkQ6ar1");

            migrationBuilder.AddColumn<string>(
                name: "password",
                table: "User",
                type: "nvarchar(256)",
                nullable: true);
        }
    }
}
