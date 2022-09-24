using BusinessObject;
using Mapster;
using Repository.Interfaces;
using Repository.Models.Recipes;
using Repository.Models.RecipeSteps;

namespace Repository
{
    public class RecipeRepository : IRecipeRepository
    {
        public async Task Add(Recipe obj)
        {
            var db = new CakeCuriousDbContext();
            db.Recipes.Add(obj);
            await db.SaveChangesAsync();
        }

        public DetailRecipeStep? GetRecipeStepDetails(Guid recipeId, int stepNumber)
        {
            var db = new CakeCuriousDbContext();
            return db.RecipeSteps
                .Where(x => x.RecipeId == recipeId && x.StepNumber == stepNumber)
                .ProjectToType<DetailRecipeStep>()
                .FirstOrDefault();
        }

        public DetailRecipe? GetRecipeDetails(Guid recipeId)
        {
            var db = new CakeCuriousDbContext();
            return db.Recipes
                .Where(x => x.Id == recipeId)
                .ProjectToType<DetailRecipe>()
                .FirstOrDefault();
        }

        // Recipe was published within 2 days
        // Recipe has an author which is followed by the follower with UID
        public ICollection<HomeRecipe> GetLatestRecipesForFollower(string uid, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Recipes
                .OrderBy(x => x.Id)
                .Where(x => x.PublishedDate!.Value <= DateTime.Now
                && x.PublishedDate!.Value >= DateTime.Now.AddDays(-2))
                .Where(x => x.User!.Followers!.Any(x => x.FollowerId == uid))
                .Skip(skip)
                .Take(take)
                .ProjectToType<HomeRecipe>()
                .ToList();
        }

        // 10 recipes for each collection
        // Collections:
        // Trending - Most liked within 1 day
        public HomeRecipes GetHomeRecipes()
        {
            var db = new CakeCuriousDbContext();
            var home = new HomeRecipes();
            var trending = db.Recipes
                .OrderByDescending(x => x.Likes!.Count)
                .Where(x => x.PublishedDate!.Value <= DateTime.Now
                && x.PublishedDate!.Value >= DateTime.Now.AddDays(-1))
                .Take(10)
                .ProjectToType<HomeRecipe>()
                .ToList();
            home.Trending = trending;
            return home;
        }
    }
}
