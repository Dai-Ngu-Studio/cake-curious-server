using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository
{
    public class UserRepository : IUserRepository
    {
        public IQueryable<User> GetList()
        {
            var db = new CakeCuriousDbContext();
            return db.Users;
        }

        public async Task<User?> Get(string uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Users.FirstOrDefaultAsync(x => x.Id == uid);
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
