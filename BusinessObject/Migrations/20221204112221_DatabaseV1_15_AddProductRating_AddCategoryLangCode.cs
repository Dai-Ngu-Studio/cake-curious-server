using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class DatabaseV1_15_AddProductRating_AddCategoryLangCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "lang_code",
                table: "ReportCategory",
                type: "varchar(128)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lang_code",
                table: "RecipeCategoryGroup",
                type: "varchar(128)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lang_code",
                table: "RecipeCategory",
                type: "varchar(128)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rating",
                table: "Product",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rating",
                table: "OrderDetail",
                type: "decimal(5,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "rating",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "rating",
                table: "OrderDetail");
        }
    }
}
