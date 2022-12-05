using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class DatabaseV1_16_RemoveLangCode_AddEnName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lang_code",
                table: "ReportCategory");

            migrationBuilder.DropColumn(
                name: "lang_code",
                table: "RecipeCategoryGroup");

            migrationBuilder.DropColumn(
                name: "lang_code",
                table: "RecipeCategory");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "ReportCategory",
                type: "nvarchar(48)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "en_name",
                table: "ReportCategory",
                type: "nvarchar(48)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "RecipeCategoryGroup",
                type: "nvarchar(48)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "en_name",
                table: "RecipeCategoryGroup",
                type: "nvarchar(48)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "RecipeCategory",
                type: "nvarchar(48)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "en_name",
                table: "RecipeCategory",
                type: "nvarchar(48)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "ProductCategory",
                type: "nvarchar(48)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "en_name",
                table: "ProductCategory",
                type: "nvarchar(48)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "en_name",
                table: "ReportCategory");

            migrationBuilder.DropColumn(
                name: "en_name",
                table: "RecipeCategoryGroup");

            migrationBuilder.DropColumn(
                name: "en_name",
                table: "RecipeCategory");

            migrationBuilder.DropColumn(
                name: "en_name",
                table: "ProductCategory");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "ReportCategory",
                type: "nvarchar(32)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(48)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lang_code",
                table: "ReportCategory",
                type: "varchar(128)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "RecipeCategoryGroup",
                type: "nvarchar(32)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(48)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lang_code",
                table: "RecipeCategoryGroup",
                type: "varchar(128)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "RecipeCategory",
                type: "nvarchar(32)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(48)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lang_code",
                table: "RecipeCategory",
                type: "varchar(128)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "ProductCategory",
                type: "nvarchar(32)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(48)",
                oldNullable: true);
        }
    }
}
