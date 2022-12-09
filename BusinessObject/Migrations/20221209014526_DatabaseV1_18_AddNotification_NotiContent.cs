using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class DatabaseV1_18_AddNotification_NotiContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationContent",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    item_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    item_type = table.Column<int>(type: "int", nullable: true),
                    item_name = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    notification_type = table.Column<int>(type: "int", nullable: true),
                    notification_date = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    title = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    en_title = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    content = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    en_content = table.Column<string>(type: "nvarchar(512)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationContent", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    content_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    user_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.id);
                    table.ForeignKey(
                        name: "FK_Notification_NotificationContent_content_id",
                        column: x => x.content_id,
                        principalTable: "NotificationContent",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Notification_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_content_id",
                table: "Notification",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_user_id",
                table: "Notification",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "NotificationContent");
        }
    }
}
