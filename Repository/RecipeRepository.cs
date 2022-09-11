using BusinessObject;
using Repository.Interfaces;

namespace Repository
{
    public class RecipeRepository : IRecipeRepository
    {
        public IQueryable<Recipe> GetList()
        {
            var db = new CakeCuriousDbContext();
            return db.Recipes;
        }

        public async Task Add(Recipe obj)
        {
            var db = new CakeCuriousDbContext();
            db.Recipes.Add(obj);
            await db.SaveChangesAsync();
        }

    }
}
