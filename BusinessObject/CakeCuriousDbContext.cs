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
            var configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString("CakeCuriousDb");
            optionsBuilder.UseSqlServer(connectionString);
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

        }
    }
}
