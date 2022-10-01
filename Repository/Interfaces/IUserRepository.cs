using BusinessObject;
using Repository.Models.Users;

namespace Repository.Interfaces
{
    public interface IUserRepository
    {
        public ICollection<User> GetList();
        public Task<User?> Get(string uid);
        public Task<DetachedUser?> GetDetached(string uid);
        public Task Add(User obj);
        public Task Update(User obj);
        public Task<ICollection<FollowerUser>> GetFollowersOfUser(string uid, string currentUserId);
        public Task<ICollection<FollowingUser>> GetFollowingOfUser(string uid, string currentUserId);
    }
}
