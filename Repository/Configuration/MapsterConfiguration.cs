using BusinessObject;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Repository.Models.Recipes;
using System.Reflection;

namespace Repository.Configuration
{
    public static class MapsterConfiguration
    {
        public static void RegisterMapsterConfiguration(this IServiceCollection services)
        {
            TypeAdapterConfig<Recipe, HomeRecipe>
                .NewConfig()
                .Map(dest => dest.Likes, src => src.Likes!.Count);

            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        }
    }
}
