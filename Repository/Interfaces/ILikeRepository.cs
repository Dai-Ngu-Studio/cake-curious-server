namespace Repository.Interfaces
{
    public interface ILikeRepository
    {
        public Task<bool> IsRecipeLikedByUser(string userId, Guid recipeId);
        public Task Remove(string userId, Guid recipeId);
        public Task Add(string userId, Guid recipeId);
    }
}
