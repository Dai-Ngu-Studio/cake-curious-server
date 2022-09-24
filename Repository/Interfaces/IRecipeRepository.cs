﻿using BusinessObject;
using Repository.Models.Recipes;

namespace Repository.Interfaces
{
    public interface IRecipeRepository
    {
        public DetailRecipe? GetRecipeDetails(Guid recipeId);
        public ICollection<HomeRecipe> GetLatestRecipesForFollower(string uid, int skip, int take);
        public HomeRecipes GetHomeRecipes();
        public Task Add(Recipe obj);
    }
}
