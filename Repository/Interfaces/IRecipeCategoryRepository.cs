using Repository.Models.RecipeCategories;
using Repository.Models.RecipeCategoryGroups;

namespace Repository.Interfaces
{
    public interface IRecipeCategoryRepository
    {
        public IEnumerable<DetachedRecipeCategory> GetRecipeCategories();
        public IEnumerable<EngDetachedRecipeCategory> GetEnglishRecipeCategories();
        public IEnumerable<DetachedRecipeCategoryGroup> GetRecipeCategoriesGrouped();
        public IEnumerable<EngDetachedRecipeCategoryGroup> GetEnglishRecipeCategoriesGrouped();
    }
}
