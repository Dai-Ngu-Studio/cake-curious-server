using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class DatabaseV1_14_AddDeactivateReason : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeactivateReason",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    item_type = table.Column<int>(type: "int", nullable: true),
                    staff_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    deactivate_date = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeactivateReason", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeactivateReason");
        }
    }
}
