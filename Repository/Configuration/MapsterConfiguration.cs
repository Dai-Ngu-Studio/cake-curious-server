using BusinessObject;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Repository.Constants.RecipeMaterials;
using Repository.Constants.RecipeMedia;
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

            TypeAdapterConfig<Recipe, DetailRecipe>
                .NewConfig()
                .Map(dest => dest.Likes, src => src.Likes!.Count)
                .Map(dest => dest.Ingredients, src => src.RecipeMaterials!.Where(x => x.MaterialType == (int)RecipeMaterialTypeEnum.Ingredient))
                .Map(dest => dest.Equipment, src => src.RecipeMaterials!.Where(x => x.MaterialType == (int)RecipeMaterialTypeEnum.Equipment))
                .Map(dest => dest.RecipeSteps, src => src.RecipeSteps!.OrderBy(x => x.StepNumber))
                .Map(dest => dest.VideoUrl, src => src.RecipeMedia!
                    .FirstOrDefault(x => 
                    x.MediaType == (int)RecipeMediaTypeEnum.Video) != null
                    ? src.RecipeMedia!.FirstOrDefault(x => x.MediaType == (int)RecipeMediaTypeEnum.Video)!.MediaUrl
                    : null);

            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        }
    }
}
