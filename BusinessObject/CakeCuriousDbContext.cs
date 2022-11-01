using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BusinessObject
{
    public class CakeCuriousDbContext : DbContext
    {
        public CakeCuriousDbContext(DbContextOptions<CakeCuriousDbContext> options) : base(options)
        {
        }

        public CakeCuriousDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString("CakeCuriousDb");
            optionsBuilder.UseSqlServer(connectionString);
            //optionsBuilder.LogTo(Console.WriteLine);
        }

        public DbSet<Bookmark> Bookmarks { get; set; } = null!;
        public DbSet<Comment> Comments { get; set; } = null!;
        public DbSet<CommentMedia> CommentMedia { get; set; } = null!;
        public DbSet<Coupon> Coupons { get; set; } = null!;
        public DbSet<Like> Likes { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductCategory> ProductCategories { get; set; } = null!;
        public DbSet<Recipe> Recipes { get; set; } = null!;
        public DbSet<RecipeCategory> RecipeCategories { get; set; } = null!;
        public DbSet<RecipeCategoryGroup> RecipeCategoryGroups { get; set; } = null!;
        public DbSet<RecipeHasCategory> RecipeHasCategories { get; set; } = null!;
        public DbSet<RecipeMaterial> RecipeMaterials { get; set; } = null!;
        public DbSet<RecipeMedia> RecipeMedia { get; set; } = null!;
        public DbSet<RecipeStep> RecipeSteps { get; set; } = null!;
        public DbSet<RecipeStepMaterial> RecipeStepMaterials { get; set; } = null!;
        public DbSet<ReportCategory> ReportCategories { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Store> Stores { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserDevice> UserDevices { get; set; } = null!;
        public DbSet<UserFollow> UserFollows { get; set; } = null!;
        public DbSet<UserHasRole> UserHasRoles { get; set; } = null!;
        public DbSet<ViolationReport> ViolationReports { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 0, Name = "Administrator", ShortName = "Admin" },
                new Role { Id = 1, Name = "Staff", ShortName = "Staff" },
                new Role { Id = 2, Name = "Store Owner", ShortName = "Store" },
                new Role { Id = 3, Name = "Baker", ShortName = "Baker" }
                );

            // Seed administrator account
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = "y0Bqpw0nQSaq4rJnZzntgmkQ6ar1",
                    Email = "admin@cakecurious.net",
                    DisplayName = "Administrator",
                    Status = 0,
                });
            modelBuilder.Entity<UserHasRole>().HasData(
                new UserHasRole { Id = Guid.Parse("248231d9-3f05-473d-9135-7be4188e0635"), RoleId = 0, UserId = "y0Bqpw0nQSaq4rJnZzntgmkQ6ar1" }
                );
        }
    }
}
