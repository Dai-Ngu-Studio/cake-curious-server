namespace Repository.Interfaces
{
    public interface IUserFollowRepository
    {
        public Task<bool> IsUserFollowedByFollower(string userId, string followerId);
        public Task Add(string userId, string followerId);
        public Task Remove(string userId, string followerId);
    }
}
