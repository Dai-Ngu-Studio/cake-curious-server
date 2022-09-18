using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class DatabaseV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Color",
                columns: table => new
                {
                    hex_code = table.Column<string>(type: "varchar(8)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Color", x => x.hex_code);
                });

            migrationBuilder.CreateTable(
                name: "Measurement",
                columns: table => new
                {
                    measurement_unit = table.Column<string>(type: "nvarchar(24)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurement", x => x.measurement_unit);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(32)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "RecipeCategoryGroup",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(32)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeCategoryGroup", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ReportCategory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(32)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportCategory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(24)", nullable: true),
                    short_name = table.Column<string>(type: "varchar(16)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "RecipeCategory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(32)", nullable: true),
                    group_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeCategory", x => x.id);
                    table.ForeignKey(
                        name: "FK_RecipeCategory_RecipeCategoryGroup_group_id",
                        column: x => x.group_id,
                        principalTable: "RecipeCategoryGroup",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Bookmark",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    recipe_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmark", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    recipe_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    content = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    submitted_date = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    root_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    depth = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.id);
                    table.ForeignKey(
                        name: "FK_Comment_Comment_root_id",
                        column: x => x.root_id,
                        principalTable: "Comment",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "CommentMedia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    comment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    media_url = table.Column<string>(type: "nvarchar(2048)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentMedia", x => x.id);
                    table.ForeignKey(
                        name: "FK_CommentMedia_Comment_comment_id",
                        column: x => x.comment_id,
                        principalTable: "Comment",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Coupon",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    store_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    name = table.Column<string>(type: "nvarchar(64)", nullable: true),
                    code = table.Column<string>(type: "varchar(24)", nullable: true),
                    discount = table.Column<decimal>(type: "decimal(20,4)", nullable: true),
                    discount_type = table.Column<int>(type: "int", nullable: true),
                    max_uses = table.Column<int>(type: "int", nullable: true),
                    expiry_date = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupon", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Like",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    recipe_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Like", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    store_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    total = table.Column<decimal>(type: "decimal(20,4)", nullable: true),
                    order_date = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    processed_date = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    completed_date = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    coupon_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.id);
                    table.ForeignKey(
                        name: "FK_Order_Coupon_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "Coupon",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    product_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    product_name = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    price = table.Column<decimal>(type: "decimal(20,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => x.id);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Order_order_id",
                        column: x => x.order_id,
                        principalTable: "Order",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    store_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    product_type = table.Column<int>(type: "int", nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    price = table.Column<decimal>(type: "decimal(20,4)", nullable: true),
                    discount = table.Column<decimal>(type: "decimal(20,4)", nullable: true),
                    photo_url = table.Column<string>(type: "nvarchar(2048)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.id);
                    table.ForeignKey(
                        name: "FK_Product_ProductCategory_category_id",
                        column: x => x.category_id,
                        principalTable: "ProductCategory",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Recipe",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    serving_size = table.Column<int>(type: "int", nullable: true),
                    photo_url = table.Column<string>(type: "nvarchar(2048)", nullable: true),
                    cook_time = table.Column<decimal>(type: "decimal(9,4)", nullable: true),
                    published_date = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipe", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "RecipeHasCategory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeHasCategory", x => x.id);
                    table.ForeignKey(
                        name: "FK_RecipeHasCategory_Recipe_recipe_id",
                        column: x => x.recipe_id,
                        principalTable: "Recipe",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_RecipeHasCategory_RecipeCategory_category_id",
                        column: x => x.category_id,
                        principalTable: "RecipeCategory",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "RecipeMaterial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    material_name = table.Column<string>(type: "nvarchar(64)", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    measurement = table.Column<string>(type: "nvarchar(24)", nullable: true),
                    color = table.Column<string>(type: "varchar(8)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeMaterial", x => x.id);
                    table.ForeignKey(
                        name: "FK_RecipeMaterial_Recipe_recipe_id",
                        column: x => x.recipe_id,
                        principalTable: "Recipe",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "RecipeMedia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    media_url = table.Column<string>(type: "nvarchar(2048)", nullable: true),
                    media_type = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeMedia", x => x.id);
                    table.ForeignKey(
                        name: "FK_RecipeMedia_Recipe_recipe_id",
                        column: x => x.recipe_id,
                        principalTable: "Recipe",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "RecipeStep",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    recipe_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    step_number = table.Column<int>(type: "int", nullable: true),
                    content = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    step_timestamp = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeStep", x => x.id);
                    table.ForeignKey(
                        name: "FK_RecipeStep_Recipe_recipe_id",
                        column: x => x.recipe_id,
                        principalTable: "Recipe",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "RecipeStepMaterial",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    step_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    material_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeStepMaterial", x => x.id);
                    table.ForeignKey(
                        name: "FK_RecipeStepMaterial_RecipeMaterial_material_id",
                        column: x => x.material_id,
                        principalTable: "RecipeMaterial",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_RecipeStepMaterial_RecipeStep_step_id",
                        column: x => x.step_id,
                        principalTable: "RecipeStep",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Store",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(128)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    longitude = table.Column<decimal>(type: "decimal(12,8)", nullable: true),
                    latitude = table.Column<decimal>(type: "decimal(12,8)", nullable: true),
                    photo_url = table.Column<string>(type: "nvarchar(2048)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    rating = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Store", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(128)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    password = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    display_name = table.Column<string>(type: "nvarchar(64)", nullable: true),
                    photo_url = table.Column<string>(type: "nvarchar(2048)", nullable: true),
                    full_name = table.Column<string>(type: "nvarchar(256)", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(16)", nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    citizenship_number = table.Column<string>(type: "varchar(24)", nullable: true),
                    citizenship_date = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    store_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.id);
                    table.ForeignKey(
                        name: "FK_User_Store_store_id",
                        column: x => x.store_id,
                        principalTable: "Store",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "UserDevice",
                columns: table => new
                {
                    token = table.Column<string>(type: "varchar(1024)", nullable: false),
                    user_id = table.Column<string>(type: "varchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevice", x => x.token);
                    table.ForeignKey(
                        name: "FK_UserDevice_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "UserFollow",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    follower_id = table.Column<string>(type: "varchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFollow", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserFollow_User_follower_id",
                        column: x => x.follower_id,
                        principalTable: "User",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_UserFollow_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "UserHasRole",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    role_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHasRole", x => x.id);
                    table.ForeignKey(
                        name: "FK_UserHasRole_Role_role_id",
                        column: x => x.role_id,
                        principalTable: "Role",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_UserHasRole_User_user_id",
                        column: x => x.user_id,
                        principalTable: "User",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ViolationReport",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "nvarchar(128)", nullable: true),
                    content = table.Column<string>(type: "nvarchar(512)", nullable: true),
                    reporter_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    staff_id = table.Column<string>(type: "varchar(128)", nullable: true),
                    item_type = table.Column<int>(type: "int", nullable: true),
                    submitted_date = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationReport", x => x.id);
                    table.ForeignKey(
                        name: "FK_ViolationReport_ReportCategory_category_id",
                        column: x => x.category_id,
                        principalTable: "ReportCategory",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ViolationReport_User_reporter_id",
                        column: x => x.reporter_id,
                        principalTable: "User",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ViolationReport_User_staff_id",
                        column: x => x.staff_id,
                        principalTable: "User",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "id", "name", "short_name" },
                values: new object[,]
                {
                    { 0, "Administrator", "Admin" },
                    { 1, "Staff", "Staff" },
                    { 2, "Store Owner", "Store" },
                    { 3, "Baker", "Baker" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmark_recipe_id",
                table: "Bookmark",
                column: "recipe_id");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmark_user_id",
                table: "Bookmark",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_recipe_id",
                table: "Comment",
                column: "recipe_id");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_root_id",
                table: "Comment",
                column: "root_id");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_user_id",
                table: "Comment",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_CommentMedia_comment_id",
                table: "CommentMedia",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_Coupon_store_id",
                table: "Coupon",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "IX_Like_recipe_id",
                table: "Like",
                column: "recipe_id");

            migrationBuilder.CreateIndex(
                name: "IX_Like_user_id",
                table: "Like",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Order_coupon_id",
                table: "Order",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "IX_Order_store_id",
                table: "Order",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "IX_Order_user_id",
                table: "Order",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_order_id",
                table: "OrderDetail",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_product_id",
                table: "OrderDetail",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_Product_category_id",
                table: "Product",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Product_store_id",
                table: "Product",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_user_id",
                table: "Recipe",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeCategory_group_id",
                table: "RecipeCategory",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeHasCategory_category_id",
                table: "RecipeHasCategory",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeHasCategory_recipe_id",
                table: "RecipeHasCategory",
                column: "recipe_id");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeMaterial_recipe_id",
                table: "RecipeMaterial",
                column: "recipe_id");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeMedia_recipe_id",
                table: "RecipeMedia",
                column: "recipe_id");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeStep_recipe_id",
                table: "RecipeStep",
                column: "recipe_id");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeStepMaterial_material_id",
                table: "RecipeStepMaterial",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeStepMaterial_step_id",
                table: "RecipeStepMaterial",
                column: "step_id");

            migrationBuilder.CreateIndex(
                name: "IX_Store_user_id",
                table: "Store",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_store_id",
                table: "User",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevice_user_id",
                table: "UserDevice",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollow_follower_id",
                table: "UserFollow",
                column: "follower_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserFollow_user_id",
                table: "UserFollow",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserHasRole_role_id",
                table: "UserHasRole",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserHasRole_user_id",
                table: "UserHasRole",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationReport_category_id",
                table: "ViolationReport",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationReport_reporter_id",
                table: "ViolationReport",
                column: "reporter_id");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationReport_staff_id",
                table: "ViolationReport",
                column: "staff_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookmark_Recipe_recipe_id",
                table: "Bookmark",
                column: "recipe_id",
                principalTable: "Recipe",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookmark_User_user_id",
                table: "Bookmark",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Recipe_recipe_id",
                table: "Comment",
                column: "recipe_id",
                principalTable: "Recipe",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_User_user_id",
                table: "Comment",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupon_Store_store_id",
                table: "Coupon",
                column: "store_id",
                principalTable: "Store",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Like_Recipe_recipe_id",
                table: "Like",
                column: "recipe_id",
                principalTable: "Recipe",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Like_User_user_id",
                table: "Like",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Store_store_id",
                table: "Order",
                column: "store_id",
                principalTable: "Store",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_User_user_id",
                table: "Order",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Product_product_id",
                table: "OrderDetail",
                column: "product_id",
                principalTable: "Product",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Store_store_id",
                table: "Product",
                column: "store_id",
                principalTable: "Store",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipe_User_user_id",
                table: "Recipe",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Store_User_user_id",
                table: "Store",
                column: "user_id",
                principalTable: "User",
                principalColumn: "id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Store_User_user_id",
                table: "Store");

            migrationBuilder.DropTable(
                name: "Bookmark");

            migrationBuilder.DropTable(
                name: "Color");

            migrationBuilder.DropTable(
                name: "CommentMedia");

            migrationBuilder.DropTable(
                name: "Like");

            migrationBuilder.DropTable(
                name: "Measurement");

            migrationBuilder.DropTable(
                name: "OrderDetail");

            migrationBuilder.DropTable(
                name: "RecipeHasCategory");

            migrationBuilder.DropTable(
                name: "RecipeMedia");

            migrationBuilder.DropTable(
                name: "RecipeStepMaterial");

            migrationBuilder.DropTable(
                name: "UserDevice");

            migrationBuilder.DropTable(
                name: "UserFollow");

            migrationBuilder.DropTable(
                name: "UserHasRole");

            migrationBuilder.DropTable(
                name: "ViolationReport");

            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "RecipeCategory");

            migrationBuilder.DropTable(
                name: "RecipeMaterial");

            migrationBuilder.DropTable(
                name: "RecipeStep");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "ReportCategory");

            migrationBuilder.DropTable(
                name: "Coupon");

            migrationBuilder.DropTable(
                name: "ProductCategory");

            migrationBuilder.DropTable(
                name: "RecipeCategoryGroup");

            migrationBuilder.DropTable(
                name: "Recipe");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Store");
        }
    }
}
