using BusinessObject;
using HotChocolate.AspNetCore.Authorization;
using Repository.Interfaces;

namespace CakeCurious_API.GraphQL
{
    public class Query
    {
        [Authorize]
        [UseOffsetPaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<User> GetUsers([Service] IUserRepository userRepository)
        {
            return userRepository.GetList();
        }

        [Authorize]
        [UseOffsetPaging]
        [UseProjection]
        [UseFiltering]
        [UseSorting]
        public IQueryable<Recipe> GetRecipes([Service] IRecipeRepository recipeRepository)
        {
            return recipeRepository.GetList();
        }
    }
}
