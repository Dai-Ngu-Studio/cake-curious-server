using BusinessObject;
using Mapster;
using Repository.Models.ProductCategories;
using Repository.Models.RecipeCategories;
using Repository.Models.RecipeCategoryGroups;
using Repository.Models.ReportCategories;

namespace Repository.Configuration.Mappings
{
    public static class CategoryMapConfiguration
    {
        public static void RegisterCategoryMapping()
        {
            // vn
            TypeAdapterConfig<RecipeCategory, DetachedRecipeCategory>
                .NewConfig()
                .Map(dest => dest.RecipeCategoryGroupType, src => src.RecipeCategoryGroup!.GroupType)
                .Map(dest => dest.RecipeCategoryGroupName, src => src.RecipeCategoryGroup!.Name);

            // en
            TypeAdapterConfig<ReportCategory, EngSimpleReportCategory>
                .NewConfig()
                .Map(dest => dest.Name, src => src.EnName);

            TypeAdapterConfig<ProductCategory, EngSimpleProductCategory>
                .NewConfig()
                .Map(dest => dest.Name, src => src.EnName);

            TypeAdapterConfig<RecipeCategory, EngDetachedRecipeCategory>
                .NewConfig()
                .Map(dest => dest.RecipeCategoryGroupType, src => src.RecipeCategoryGroup!.GroupType)
                .Map(dest => dest.RecipeCategoryGroupName, src => src.RecipeCategoryGroup!.EnName)
                .Map(dest => dest.Name, src => src.EnName);

            TypeAdapterConfig<RecipeCategoryGroup, EngDetachedRecipeCategoryGroup>
                .NewConfig()
                .Map(dest => dest.Name, src => src.EnName);
        }
    }
}
