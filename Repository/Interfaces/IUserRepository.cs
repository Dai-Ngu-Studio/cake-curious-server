using BusinessObject;
using Repository.Models.Users;

namespace Repository.Interfaces
{
    public interface IUserRepository
    {
        public Task<IEnumerable<AdminDashboardUser>?> GetList(string? search, string? order_by, string? filter, int page, int size);
        public Task<User?> Get(string uid);
        public Task<DetachedUser?> GetDetached(string uid);
        public Task Add(User obj);
        public Task Update(User obj);
        public Task<int> CountFollowersOfUser(string uid);
        public Task<ICollection<FollowerUser>> GetFollowersOfUser(string uid, string currentUserId, int skip, int take);
        public Task<int> CountFollowingOfUser(string uid);
        public Task<ICollection<FollowingUser>> GetFollowingOfUser(string uid, string currentUserId, int skip, int take);
        public int CountDashboardUser(string? search, string? order_by, string? filter);
        public Task<UserDetailForWeb?> GetUserDetailForWeb(string uid);
        public Task<User?> DeleteUser(string? id);
        public Task<ProfileUser?> GetProfileUser(string? id);
    }
}
