using BusinessObject;
using Repository.Models.Recipes;
using Repository.Models.RecipeSteps;

namespace Repository.Interfaces
{
    public interface IRecipeRepository
    {
        public Task<DetailRecipeStep?> GetRecipeStepDetails(Guid recipeId, int stepNumber);
        public Task<DetailRecipe?> GetRecipeDetails(Guid recipeId);
        public int CountLatestRecipesForFollower(string uid);
        public IEnumerable<HomeRecipe> GetLatestRecipesForFollower(string uid, int skip, int take);
        public HomeRecipes GetHomeRecipes();
        public Task Add(Recipe obj);
    }
}
