using BusinessObject;

namespace Repository.Interfaces
{
    public interface IBookmarkRepository
    {
        public Task<bool> IsRecipeBookmarkedByUser(string userId, Guid recipeId);
        public Task Remove(string userId, Guid recipeId);
        public Task Add(string userId, Guid recipeId);
    }
}
