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

        public async Task<User?> Get(string uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Users.FirstOrDefaultAsync(x => x.Id == uid);
        }

        public async Task<DetachedUser?> GetDetached(string uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Users.Where(x => x.Id == uid).ProjectToType<DetachedUser>().FirstOrDefaultAsync();
        }

        public async Task Add(User obj)
        {
            var db = new CakeCuriousDbContext();
            db.Users.Add(obj);
            await db.SaveChangesAsync();
        }

        public Task Update(User obj)
        {
            throw new NotImplementedException();
        }
    }
}
