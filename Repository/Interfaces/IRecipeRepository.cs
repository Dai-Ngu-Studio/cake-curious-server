using BusinessObject;
using Repository.Models.Recipes;
using Repository.Models.RecipeSteps;

namespace Repository.Interfaces
{
    public interface IRecipeRepository
    {
        public DetailRecipeStep? GetRecipeStepDetails(Guid recipeId, int stepNumber);
        public DetailRecipe? GetRecipeDetails(Guid recipeId);
        public ICollection<HomeRecipe> GetLatestRecipesForFollower(string uid, int skip, int take);
        public HomeRecipes GetHomeRecipes();
        public Task Add(Recipe obj);
    }
}
