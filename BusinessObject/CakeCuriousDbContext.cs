﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BusinessObject
{
    public class CakeCuriousDbContext : DbContext
    {
        public CakeCuriousDbContext(DbContextOptions<CakeCuriousDbContext> options = null!) : base(options)
        {
        }

        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<CommentImage> CommentImages { get; set; } = null!;
        public DbSet<Coupon> Coupons { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductType> ProductTypes { get; set; } = null!;
        public DbSet<Recipe> Recipes { get; set; } = null!;
        public DbSet<RecipeBakingMaterial> RecipeBakingMaterials { get; set; } = null!;
        public DbSet<RecipeCategory> RecipeCategories { get; set; } = null!;
        public DbSet<RecipeHasCategory> RecipeHasCategories { get; set; } = null!;
        public DbSet<RecipeStep> RecipeSteps { get; set; } = null!;
        public DbSet<RecipeVisualMaterial> RecipeVisualMaterials { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Store> Stores { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserDevice> UserDevices { get; set; } = null!;
        public DbSet<ViolationReport> ViolationReports { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Recipe>()
                .HasOne(x => x.Author)
                .WithMany(x => x.Recipes)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Store>()
                .HasOne(x => x.Owner)
                .WithMany(x => x.Stores)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserDevice>()
                .HasOne(x => x.User)
                .WithMany(x => x.UserDevices)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ViolationReport>()
                .HasOne(x => x.Reporter)
                .WithMany(x => x.ViolationReports)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ViolationReport>()
                .HasOne(x => x.Staff)
                .WithMany(x => x.ResolvedViolationReports)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Comment>()
                .HasOne(x => x.Recipe)
                .WithMany(x => x.Comments)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Comment>()
                .HasOne(x => x.Author)
                .WithMany(x => x.Comments)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RecipeBakingMaterial>()
                .HasOne(x => x.Recipe)
                .WithMany(x => x.BakingMaterials)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RecipeHasCategory>()
                .HasOne(x => x.Category)
                .WithMany(x => x.HasRecipes)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RecipeHasCategory>()
                .HasOne(x => x.Recipe)
                .WithMany(x => x.HasCategories)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RecipeStep>()
                .HasOne(x => x.Recipe)
                .WithMany(x => x.Steps)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<RecipeVisualMaterial>()
                .HasOne(x => x.Recipe)
                .WithMany(x => x.VisualMaterials)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Coupon>()
                .HasOne(x => x.Store)
                .WithMany(x => x.Coupons)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(x => x.Store)
                .WithMany(x => x.Orders)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(x => x.Buyer)
                .WithMany(x => x.Orders)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Order>()
                .HasOne(x => x.Coupon)
                .WithMany(x => x.Orders)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .HasOne(x => x.Store)
                .WithMany(x => x.Products)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Product>()
                .HasOne(x => x.ProductType)
                .WithMany(x => x.Products)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CommentImage>()
                .HasOne(x => x.Comment)
                .WithMany(x => x.Images)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(x => x.Order)
                .WithMany(x => x.OrderDetails)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(x => x.Product)
                .WithMany(x => x.OrderDetails)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }
}
