using BusinessObject;
using Repository.Models.RecipeMaterials;
using Repository.Models.Recipes;
using Repository.Models.RecipeSteps;

namespace Repository.Interfaces
{
    public interface IRecipeRepository
    {
        public Task<DetailRecipeStep?> GetRecipeStepDetails(Guid recipeId, int stepNumber);
        public Task<DetailRecipe?> GetRecipeDetails(Guid recipeId, string userId);
        public Task<int> CountLatestRecipesForFollower(string uid);
        public IEnumerable<HomeRecipe> GetLatestRecipesForFollower(string uid, int skip, int take);
        public Task<int> CountTrendingRecipes(int period);
        public IEnumerable<HomeRecipe> GetTrendingRecipes(int period, int skip, int take);
        public Task AddRecipe(Recipe obj);
        public Task<ICollection<ExploreRecipe>> Explore(int randSeed, int skip, int take);
        public Task<int> Delete(Guid id);
        public Task<Recipe?> GetRecipeReadonly(Guid id);
        public Task<IEnumerable<SimpleRecipeForReportList>> GetReportedRecipes(string? s, string? sort, string? filter, int page, int size);
        public Task<int?> CountTotalReportedRecipes(string? s, string? sort, string? filter);
        public Task<Recipe?> GetRecipeWithStepsReadonly(Guid id);
        public Task<Recipe?> GetRecipe(Guid id);
        public Task<EditRecipe?> GetEditRecipe(Guid id);
        public Task UpdateRecipe(Recipe recipe, Recipe updateRecipe);
        public Task<int> CountBookmarksOfUser(string userId);
        public IEnumerable<HomeRecipe> GetBookmarksOfUser(string userId, int skip, int take);
        public Task<int> CountLikedOfUser(string userId);
        public IEnumerable<HomeRecipe> GetLikedOfUser(string userId, int skip, int take);
        public Task<int> CountRecipesOfUser(string userId);
        public IEnumerable<HomeRecipe> GetRecipesOfUser(string userId, int skip, int take);
        public Task<ICollection<HomeRecipe>> GetSuggestedRecipes(List<Guid> recipeIds);
        public Task<int> UpdateShareUrl(Guid id, string shareUrl);
    }
}
