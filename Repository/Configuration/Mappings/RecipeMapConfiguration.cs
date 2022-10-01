﻿using BusinessObject;
using Mapster;
using Repository.Constants.RecipeMaterials;
using Repository.Constants.RecipeMedia;
using Repository.Models.Recipes;

namespace Repository.Configuration.Mappings
{
    public static class RecipeMapConfiguration
    {
        public static void RegisterRecipeMapping()
        {
            TypeAdapterConfig<Recipe, HomeRecipe>
                .NewConfig()
                .Map(dest => dest.Likes, src => src.Likes!.Count);

            TypeAdapterConfig<Recipe, DetailRecipe>
                .NewConfig()
                .Map(dest => dest.IsLikedByCurrentUser, src => src.Likes!
                    .Any(x => x.UserId == (string)MapContext.Current!.Parameters["userId"]))
                .Map(dest => dest.IsBookmarkedByCurrentUser, src => src.Bookmarks!
                    .Any(x => x.UserId == (string)MapContext.Current!.Parameters["userId"]))
                .Map(dest => dest.Likes, src => src.Likes!.Count)
                .Map(dest => dest.Ingredients, src => src.RecipeMaterials!.Where(x => x.MaterialType == (int)RecipeMaterialTypeEnum.Ingredient))
                .Map(dest => dest.Equipment, src => src.RecipeMaterials!.Where(x => x.MaterialType == (int)RecipeMaterialTypeEnum.Equipment))
                .Map(dest => dest.RecipeSteps, src => src.RecipeSteps!.OrderBy(x => x.StepNumber))
                .Map(dest => dest.VideoUrl, src => src.RecipeMedia!
                    .FirstOrDefault(x =>
                    x.MediaType == (int)RecipeMediaTypeEnum.Video) != null
                    ? src.RecipeMedia!.FirstOrDefault(x => x.MediaType == (int)RecipeMediaTypeEnum.Video)!.MediaUrl
                    : null);

            TypeAdapterConfig<CreateRecipe, Recipe>
                .NewConfig()
                .Map(dest => dest.RecipeMaterials, src => src.Ingredients!.Concat(src.Equipment!));
        }
    }
}
