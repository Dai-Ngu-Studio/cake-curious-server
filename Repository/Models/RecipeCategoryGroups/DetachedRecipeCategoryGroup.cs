using Repository.Models.RecipeCategories;

namespace Repository.Models.RecipeCategoryGroups
{
    public class DetachedRecipeCategoryGroup
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? LangCode { get; set; }
        public int? GroupType { get; set; }
        public IEnumerable<DetachedRecipeCategory>? RecipeCategories { get; set; }
    }
}
