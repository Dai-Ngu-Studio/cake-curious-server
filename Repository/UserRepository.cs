using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.Users;

namespace Repository
{
    public class UserRepository : IUserRepository
    {
        public ICollection<User> GetList()
        {
            var db = new CakeCuriousDbContext();
            return db.Users.ToList();
        }

        public async Task<ICollection<FollowUser>> GetFollowersOfUser(string uid, string currentUserId)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("userId", currentUserId);

                var db = new CakeCuriousDbContext();
                return await db.UserFollows
                    .Where(x => x.UserId == uid)
                    .ProjectToType<FollowUser>()
                    .ToListAsync();
            }
        }

        public async Task<User?> Get(string uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Users.Include(x => x.HasRoles).FirstOrDefaultAsync(x => x.Id == uid);
        }

        public async Task<DetachedUser?> GetDetached(string uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Users.Where(x => x.Id == uid).ProjectToType<DetachedUser>().FirstOrDefaultAsync();
        }

        public async Task Add(User obj)
        {
            var db = new CakeCuriousDbContext();
            await db.Users.AddAsync(obj);
            await db.SaveChangesAsync();
        }

        public async Task Update(User obj)
        {
            var db = new CakeCuriousDbContext();
            db.Users.Update(obj);
            await db.SaveChangesAsync();
        }
    }
}
