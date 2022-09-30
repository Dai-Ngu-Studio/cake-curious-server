namespace Repository.Interfaces
{
    public interface ILikeRepository
    {
        public Task<bool> IsRecipeLikedByUser(string userId, Guid recipeId);
        public Task RemoveLike(string userId, Guid recipeId);
        public Task AddLike(string userId, Guid recipeId);
    }
}
