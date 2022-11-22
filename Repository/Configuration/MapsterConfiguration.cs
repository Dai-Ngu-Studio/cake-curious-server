using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Repository.Configuration.Mappings;
using System.Reflection;

namespace Repository.Configuration
{
    public static class MapsterConfiguration
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            RecipeMapConfiguration.RegisterRecipeMapping();
            RecipeStepMapConfiguration.RegisterRecipeStepMapping();
            CommentMapConfiguration.RegisterCommentMapping();
            UserMapConfiguration.RegisterUserMapping();
            CouponMapConfiguration.RegisterCouponMapping();
            StoreMapConfiguration.RegisterStoreMapping();
            OrderMapConfiguration.RegisterOrderMapping();
            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        }
    }
}
