using BusinessObject;
using Mapster;
using Repository.Constants.RecipeMaterials;
using Repository.Models.RecipeSteps;

namespace Repository.Configuration.Mappings
{
    public static class RecipeStepMapConfiguration
    {
        public static void RegisterRecipeStepMapping()
        {
            TypeAdapterConfig<RecipeStep, DetailRecipeStep>
                .NewConfig()
                .Map(dest => dest.Ingredients, src => src.RecipeStepMaterials!.Where(x => x.RecipeMaterial!.MaterialType == (int)RecipeMaterialTypeEnum.Ingredient)
                    .Select(x => x.RecipeMaterial))
                .Map(dest => dest.Equipment, src => src.RecipeStepMaterials!.Where(x => x.RecipeMaterial!.MaterialType == (int)RecipeMaterialTypeEnum.Equipment)
                    .Select(x => x.RecipeMaterial));
        }
    }
}
