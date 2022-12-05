using BusinessObject;
using Mapster;
using Repository.Interfaces;
using Repository.Models.RecipeCategories;
using Repository.Models.RecipeCategoryGroups;

namespace Repository
{
    public class RecipeCategoryRepository : IRecipeCategoryRepository
    {
        public IEnumerable<DetachedRecipeCategory> GetRecipeCategories()
        {
            var db = new CakeCuriousDbContext();
            return db.RecipeCategories.ProjectToType<DetachedRecipeCategory>();
        }

        public IEnumerable<EngDetachedRecipeCategory> GetEnglishRecipeCategories()
        {
            var db = new CakeCuriousDbContext();
            return db.RecipeCategories.ProjectToType<EngDetachedRecipeCategory>();
        }

        public IEnumerable<DetachedRecipeCategoryGroup> GetRecipeCategoriesGrouped()
        {
            var db = new CakeCuriousDbContext();
            return db.RecipeCategoryGroups.ProjectToType<DetachedRecipeCategoryGroup>();
        }

        public IEnumerable<EngDetachedRecipeCategoryGroup> GetEnglishRecipeCategoriesGrouped()
        {
            var db = new CakeCuriousDbContext();
            return db.RecipeCategoryGroups.ProjectToType<EngDetachedRecipeCategoryGroup>();
        }
    }
}
