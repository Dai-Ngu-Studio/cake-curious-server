using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class DatabaseV1_19_AddMissingFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DeactivateReason_staff_id",
                table: "DeactivateReason",
                column: "staff_id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeactivateReason_User_staff_id",
                table: "DeactivateReason",
                column: "staff_id",
                principalTable: "User",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeactivateReason_User_staff_id",
                table: "DeactivateReason");

            migrationBuilder.DropIndex(
                name: "IX_DeactivateReason_staff_id",
                table: "DeactivateReason");
        }
    }
}
