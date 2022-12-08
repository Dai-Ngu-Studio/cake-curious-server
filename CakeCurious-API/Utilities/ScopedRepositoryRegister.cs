using Repository;
using Repository.Interfaces;

namespace CakeCurious_API.Utilities
{
    public class ScopedRepositoryRegister
    {
        public static void AddScopedRepositories(IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IStoreRepository, StoreRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IViolationReportRepository, ReportRepository>();
            services.AddScoped<ILikeRepository, LikeRepository>();
            services.AddScoped<IBookmarkRepository, BookmarkRepository>();
            services.AddScoped<IUserFollowRepository, UserFollowRepository>();
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddScoped<IRecipeCategoryRepository, RecipeCategoryRepository>();
            services.AddScoped<IDashboardReportRepository, DashboardReportRepository>();
            services.AddScoped<IReportCategoryRepository, ReportCategoryRepository>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
        }
    }
}
