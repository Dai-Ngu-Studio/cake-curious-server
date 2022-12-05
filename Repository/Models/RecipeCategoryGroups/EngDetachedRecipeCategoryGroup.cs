using Repository.Models.RecipeCategories;

namespace Repository.Models.RecipeCategoryGroups
{
    public class EngDetachedRecipeCategoryGroup
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public int? GroupType { get; set; }
        public IEnumerable<EngDetachedRecipeCategory>? RecipeCategories { get; set; }
    }
}
