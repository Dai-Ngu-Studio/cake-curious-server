using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository
{
    public class UserFollowRepository : IUserFollowRepository
    {
        public async Task<bool> IsUserFollowedByFollower(string userId, string followerId)
        {
            var db = new CakeCuriousDbContext();
            return await db.UserFollows.AnyAsync(x => x.UserId == userId && x.FollowerId == followerId);
        }

        public async Task Add(string userId, string followerId)
        {
            var db = new CakeCuriousDbContext();
            await db.UserFollows.AddAsync(new UserFollow
            {
                UserId = userId,
                FollowerId = followerId,
            });
            await db.SaveChangesAsync();
        }

        public async Task Remove(string userId, string followerId)
        {
            var db = new CakeCuriousDbContext();
            var follow = await db.UserFollows.FirstOrDefaultAsync(x => x.UserId == userId && x.FollowerId == followerId);
            db.UserFollows.Remove(follow!);
            await db.SaveChangesAsync();
        }
    }
}
