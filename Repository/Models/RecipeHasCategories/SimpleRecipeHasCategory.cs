using Repository.Models.RecipeCategories;

namespace Repository.Models.RecipeHasCategories
{
    public class SimpleRecipeHasCategory
    {
        public Guid Id { get; set; }
        public int? RecipeCategoryId { get; set; }
        public DetachedRecipeCategory? RecipeCategory { get; set; }

    }
}
