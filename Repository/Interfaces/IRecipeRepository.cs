﻿using BusinessObject;
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
        public HomeRecipes GetHomeRecipes();
        public Task AddRecipe(Recipe obj, IEnumerable<CreateRecipeMaterial> recipeMaterials);
        public Task<ICollection<ExploreRecipe>> Explore(int randSeed, int skip, int take);
    }
}
