using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.RecipeMaterials;
using Repository.Models.Recipes;
using Repository.Models.RecipeSteps;

namespace Repository
{
    public class RecipeRepository : IRecipeRepository
    {
        public async Task AddRecipe(Recipe obj, IEnumerable<CreateRecipeMaterial> recipeMaterials)
        {
            var db = new CakeCuriousDbContext();
            // Add recipe, materials and steps
            await db.Recipes.AddAsync(obj);
            // Add relationship between material and step
            var stepMaterials = new List<RecipeStepMaterial>();
            foreach (var material in recipeMaterials)
            {
                if (material.UsedInSteps != null)
                {
                    foreach (var step in material.UsedInSteps)
                    {
                        var recipeStep = obj.RecipeSteps!.FirstOrDefault(x => x.StepNumber == step);
                        if (recipeStep != null)
                        {
                            stepMaterials.Add(new RecipeStepMaterial
                            {
                                RecipeMaterialId = material.Id,
                                RecipeStepId = recipeStep.Id,
                            });
                        }
                    }
                }
            }
            await db.RecipeStepMaterials.AddRangeAsync(stepMaterials);
            await db.SaveChangesAsync();
        }

        public async Task<DetailRecipeStep?> GetRecipeStepDetails(Guid recipeId, int stepNumber)
        {
            var db = new CakeCuriousDbContext();
            return await db.RecipeSteps
                .Where(x => x.RecipeId == recipeId && x.StepNumber == stepNumber)
                .ProjectToType<DetailRecipeStep>()
                .FirstOrDefaultAsync();
        }

        public async Task<DetailRecipe?> GetRecipeDetails(Guid recipeId, string userId)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("userId", userId);

                var db = new CakeCuriousDbContext();
                return await db.Recipes
                    .Where(x => x.Id == recipeId)
                    .ProjectToType<DetailRecipe>()
                    .FirstOrDefaultAsync();
            }
        }

        public int CountLatestRecipesForFollower(string uid)
        {
            var db = new CakeCuriousDbContext();
            return db.Recipes
                .OrderBy(x => x.Id)
                .Where(x => x.PublishedDate!.Value <= DateTime.Now
                && x.PublishedDate!.Value >= DateTime.Now.AddDays(-2))
                .Where(x => x.User!.Followers!.Any(x => x.FollowerId == uid))
                .Count();
        }

        // Recipe was published within 2 days
        // Recipe has an author which is followed by the follower with UID
        public IEnumerable<HomeRecipe> GetLatestRecipesForFollower(string uid, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Recipes
                .OrderBy(x => x.Id)
                .Where(x => x.PublishedDate!.Value <= DateTime.Now
                && x.PublishedDate!.Value >= DateTime.Now.AddDays(-2))
                .Where(x => x.User!.Followers!.Any(x => x.FollowerId == uid))
                .Skip(skip)
                .Take(take)
                .ProjectToType<HomeRecipe>();
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
                .ProjectToType<HomeRecipe>();
            home.Trending = trending;
            return home;
        }
    }
}
