﻿// <auto-generated />
using System;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BusinessObject.Migrations
{
    [DbContext(typeof(CakeCuriousDbContext))]
    [Migration("20220911094249_NullabilityUpdate")]
    partial class NullabilityUpdate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("BusinessObject.Comment", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("AuthorId")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("author_id");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(512)")
                        .HasColumnName("content");

                    b.Property<Guid?>("RecipeId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("recipe_id");

                    b.Property<int?>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.Property<DateTime?>("SubmittedDate")
                        .HasColumnType("datetime2(7)")
                        .HasColumnName("submitted_date");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("RecipeId");

                    b.ToTable("Comment");
                });

            modelBuilder.Entity("BusinessObject.CommentImage", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<Guid?>("CommentId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("comment_id");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("image_url");

                    b.HasKey("Id");

                    b.HasIndex("CommentId");

                    b.ToTable("CommentImage");
                });

            modelBuilder.Entity("BusinessObject.Coupon", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("Code")
                        .HasColumnType("varchar(24)")
                        .HasColumnName("code");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2(7)")
                        .HasColumnName("expiry_date");

                    b.Property<int?>("MaxUses")
                        .HasColumnType("int")
                        .HasColumnName("max_uses");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("name");

                    b.Property<int?>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.Property<Guid?>("StoreId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("store_id");

                    b.Property<int?>("UsedCount")
                        .HasColumnType("int")
                        .HasColumnName("used_count");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("Coupon");
                });

            modelBuilder.Entity("BusinessObject.Order", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("BuyerId")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("buyer_id");

                    b.Property<DateTime?>("CompletedDate")
                        .HasColumnType("datetime2(7)")
                        .HasColumnName("completed_date");

                    b.Property<string>("CouponCode")
                        .HasColumnType("varchar(24)")
                        .HasColumnName("coupon_code");

                    b.Property<Guid?>("CouponId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("coupon_id");

                    b.Property<DateTime?>("OrderDate")
                        .HasColumnType("datetime2(7)")
                        .HasColumnName("order_date");

                    b.Property<DateTime?>("ProcessedDate")
                        .HasColumnType("datetime2(7)")
                        .HasColumnName("processed_date");

                    b.Property<int?>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.Property<Guid?>("StoreId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("store_id");

                    b.Property<decimal?>("Subtotal")
                        .HasColumnType("decimal(20,4)")
                        .HasColumnName("subtotal");

                    b.HasKey("Id");

                    b.HasIndex("BuyerId");

                    b.HasIndex("CouponId");

                    b.HasIndex("StoreId");

                    b.ToTable("Order");
                });

            modelBuilder.Entity("BusinessObject.OrderDetail", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<int?>("Amount")
                        .HasColumnType("int")
                        .HasColumnName("amount");

                    b.Property<Guid?>("OrderId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("order_id");

                    b.Property<decimal?>("Price")
                        .HasColumnType("decimal(20,4)")
                        .HasColumnName("price");

                    b.Property<Guid?>("ProductId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("product_id");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("product_name");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("OrderDetail");
                });

            modelBuilder.Entity("BusinessObject.Product", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(512)")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("name");

                    b.Property<decimal?>("Price")
                        .HasColumnType("decimal(20,4)")
                        .HasColumnName("price");

                    b.Property<Guid?>("ProductTypeId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("type_id");

                    b.Property<int?>("Quantity")
                        .HasColumnType("int")
                        .HasColumnName("quantity");

                    b.Property<int?>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.Property<Guid?>("StoreId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("store_id");

                    b.Property<Guid?>("TypeId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.HasIndex("TypeId");

                    b.ToTable("Product");
                });

            modelBuilder.Entity("BusinessObject.ProductType", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("ProductType");
                });

            modelBuilder.Entity("BusinessObject.Recipe", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("AuthorId")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("author_id");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(512)")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("name");

                    b.Property<string>("PhotoUrl")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("photo_url");

                    b.Property<DateTime?>("PublishedDate")
                        .HasColumnType("datetime2(7)")
                        .HasColumnName("published_date");

                    b.Property<int?>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.Property<decimal?>("TimeNeeded")
                        .HasColumnType("decimal(9,4)")
                        .HasColumnName("time_needed");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("Recipe");
                });

            modelBuilder.Entity("BusinessObject.RecipeBakingMaterial", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("Amount")
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("amount");

                    b.Property<string>("MaterialName")
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("material_name");

                    b.Property<Guid?>("RecipeId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("recipe_id");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId");

                    b.ToTable("RecipeBakingMaterial");
                });

            modelBuilder.Entity("BusinessObject.RecipeCategory", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(128)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("RecipeCategory");
                });

            modelBuilder.Entity("BusinessObject.RecipeHasCategory", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<Guid?>("CategoryId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("category_id");

                    b.Property<Guid?>("RecipeId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("recipe_id");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("RecipeId");

                    b.ToTable("RecipeHasCategory");
                });

            modelBuilder.Entity("BusinessObject.RecipeStep", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(512)")
                        .HasColumnName("content");

                    b.Property<Guid?>("RecipeId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("recipe_id");

                    b.Property<int?>("StepNumber")
                        .HasColumnType("int")
                        .HasColumnName("step_number");

                    b.Property<int?>("StepTimestamp")
                        .HasColumnType("int")
                        .HasColumnName("step_timestamp");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId");

                    b.ToTable("RecipeStep");
                });

            modelBuilder.Entity("BusinessObject.RecipeVisualMaterial", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("MaterialUrl")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("material_url");

                    b.Property<Guid?>("RecipeId")
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("recipe_id");

                    b.Property<int?>("VisualType")
                        .HasColumnType("int")
                        .HasColumnName("visual_type");

                    b.HasKey("Id");

                    b.HasIndex("RecipeId");

                    b.ToTable("RecipeVisualMaterial");
                });

            modelBuilder.Entity("BusinessObject.Role", b =>
                {
                    b.Property<int?>("Id")
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(256)")
                        .HasColumnName("name");

                    b.Property<string>("ShortName")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("short_name");

                    b.HasKey("Id");

                    b.ToTable("Role");
                });

            modelBuilder.Entity("BusinessObject.Store", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(512)")
                        .HasColumnName("description");

                    b.Property<decimal?>("Latitude")
                        .HasColumnType("decimal(12,8)")
                        .HasColumnName("latitude");

                    b.Property<decimal?>("Longitude")
                        .HasColumnType("decimal(12,8)")
                        .HasColumnName("longitude");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("name");

                    b.Property<string>("OwnerId")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("owner_id");

                    b.Property<string>("PhotoUrl")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("photo_url");

                    b.Property<decimal?>("Rating")
                        .HasColumnType("decimal(8,2)")
                        .HasColumnName("rating");

                    b.Property<int?>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Store");
                });

            modelBuilder.Entity("BusinessObject.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("id");

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("display_name");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("email");

                    b.Property<string>("PhotoUrl")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("photo_url");

                    b.Property<int?>("RoleId")
                        .HasColumnType("int")
                        .HasColumnName("role_id");

                    b.Property<int?>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("User");
                });

            modelBuilder.Entity("BusinessObject.UserDevice", b =>
                {
                    b.Property<string>("Token")
                        .HasColumnType("varchar(1024)")
                        .HasColumnName("token");

                    b.Property<string>("UserId")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("user_id");

                    b.HasKey("Token");

                    b.HasIndex("UserId");

                    b.ToTable("UserDevice");
                });

            modelBuilder.Entity("BusinessObject.ViolationReport", b =>
                {
                    b.Property<Guid?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasColumnName("id");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(512)")
                        .HasColumnName("content");

                    b.Property<int>("ItemType")
                        .HasColumnType("int")
                        .HasColumnName("item_type");

                    b.Property<string>("ReporterId")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("reporter_id");

                    b.Property<string>("StaffId")
                        .HasColumnType("varchar(128)")
                        .HasColumnName("staff_id");

                    b.Property<int>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.Property<DateTime?>("SubmittedDate")
                        .HasColumnType("datetime2(7)")
                        .HasColumnName("submitted_date");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(256)")
                        .HasColumnName("title");

                    b.HasKey("Id");

                    b.HasIndex("ReporterId");

                    b.HasIndex("StaffId");

                    b.ToTable("ViolationReport");
                });

            modelBuilder.Entity("BusinessObject.Comment", b =>
                {
                    b.HasOne("BusinessObject.User", "Author")
                        .WithMany("Comments")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("BusinessObject.Recipe", "Recipe")
                        .WithMany("Comments")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Author");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("BusinessObject.CommentImage", b =>
                {
                    b.HasOne("BusinessObject.Comment", "Comment")
                        .WithMany("Images")
                        .HasForeignKey("CommentId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Comment");
                });

            modelBuilder.Entity("BusinessObject.Coupon", b =>
                {
                    b.HasOne("BusinessObject.Store", "Store")
                        .WithMany("Coupons")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Store");
                });

            modelBuilder.Entity("BusinessObject.Order", b =>
                {
                    b.HasOne("BusinessObject.User", "Buyer")
                        .WithMany("Orders")
                        .HasForeignKey("BuyerId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("BusinessObject.Coupon", "Coupon")
                        .WithMany("Orders")
                        .HasForeignKey("CouponId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("BusinessObject.Store", "Store")
                        .WithMany("Orders")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Buyer");

                    b.Navigation("Coupon");

                    b.Navigation("Store");
                });

            modelBuilder.Entity("BusinessObject.OrderDetail", b =>
                {
                    b.HasOne("BusinessObject.Order", "Order")
                        .WithMany("OrderDetails")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("BusinessObject.Product", "Product")
                        .WithMany("OrderDetails")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("BusinessObject.Product", b =>
                {
                    b.HasOne("BusinessObject.Store", "Store")
                        .WithMany("Products")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("BusinessObject.ProductType", "ProductType")
                        .WithMany("Products")
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("ProductType");

                    b.Navigation("Store");
                });

            modelBuilder.Entity("BusinessObject.Recipe", b =>
                {
                    b.HasOne("BusinessObject.User", "Author")
                        .WithMany("Recipes")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Author");
                });

            modelBuilder.Entity("BusinessObject.RecipeBakingMaterial", b =>
                {
                    b.HasOne("BusinessObject.Recipe", "Recipe")
                        .WithMany("BakingMaterials")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("BusinessObject.RecipeHasCategory", b =>
                {
                    b.HasOne("BusinessObject.RecipeCategory", "Category")
                        .WithMany("HasRecipes")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("BusinessObject.Recipe", "Recipe")
                        .WithMany("HasCategories")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Category");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("BusinessObject.RecipeStep", b =>
                {
                    b.HasOne("BusinessObject.Recipe", "Recipe")
                        .WithMany("Steps")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("BusinessObject.RecipeVisualMaterial", b =>
                {
                    b.HasOne("BusinessObject.Recipe", "Recipe")
                        .WithMany("VisualMaterials")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("BusinessObject.Store", b =>
                {
                    b.HasOne("BusinessObject.User", "Owner")
                        .WithMany("Stores")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("BusinessObject.User", b =>
                {
                    b.HasOne("BusinessObject.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Role");
                });

            modelBuilder.Entity("BusinessObject.UserDevice", b =>
                {
                    b.HasOne("BusinessObject.User", "User")
                        .WithMany("UserDevices")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("User");
                });

            modelBuilder.Entity("BusinessObject.ViolationReport", b =>
                {
                    b.HasOne("BusinessObject.User", "Reporter")
                        .WithMany("ViolationReports")
                        .HasForeignKey("ReporterId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("BusinessObject.User", "Staff")
                        .WithMany("ResolvedViolationReports")
                        .HasForeignKey("StaffId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Reporter");

                    b.Navigation("Staff");
                });

            modelBuilder.Entity("BusinessObject.Comment", b =>
                {
                    b.Navigation("Images");
                });

            modelBuilder.Entity("BusinessObject.Coupon", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("BusinessObject.Order", b =>
                {
                    b.Navigation("OrderDetails");
                });

            modelBuilder.Entity("BusinessObject.Product", b =>
                {
                    b.Navigation("OrderDetails");
                });

            modelBuilder.Entity("BusinessObject.ProductType", b =>
                {
                    b.Navigation("Products");
                });

            modelBuilder.Entity("BusinessObject.Recipe", b =>
                {
                    b.Navigation("BakingMaterials");

                    b.Navigation("Comments");

                    b.Navigation("HasCategories");

                    b.Navigation("Steps");

                    b.Navigation("VisualMaterials");
                });

            modelBuilder.Entity("BusinessObject.RecipeCategory", b =>
                {
                    b.Navigation("HasRecipes");
                });

            modelBuilder.Entity("BusinessObject.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("BusinessObject.Store", b =>
                {
                    b.Navigation("Coupons");

                    b.Navigation("Orders");

                    b.Navigation("Products");
                });

            modelBuilder.Entity("BusinessObject.User", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("Orders");

                    b.Navigation("Recipes");

                    b.Navigation("ResolvedViolationReports");

                    b.Navigation("Stores");

                    b.Navigation("UserDevices");

                    b.Navigation("ViolationReports");
                });
#pragma warning restore 612, 618
        }
    }
}
