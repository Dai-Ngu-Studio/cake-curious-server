using BusinessObject;
using Repository.Models.Recipes;

namespace Repository.Interfaces
{
    public interface IRecipeRepository
    {
        public ICollection<FollowRecipe> GetLatestRecipesForFollower(string uid, int skip, int take);
        public Task Add(Recipe obj);
    }
}
