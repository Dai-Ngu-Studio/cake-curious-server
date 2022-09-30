using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository
{
    public class UserDeviceRepository : IUserDeviceRepository
    {
        public async Task Add(UserDevice obj)
        {
            var db = new CakeCuriousDbContext();
            await db.UserDevices.AddAsync(obj);
            await db.SaveChangesAsync();
        }

        public async Task<UserDevice?> Get(string token)
        {
            var db = new CakeCuriousDbContext();
            return await db.UserDevices.FirstOrDefaultAsync(x => x.Token == token);
        }

        public ICollection<UserDevice> GetList()
        {
            throw new NotImplementedException();
        }
    }
}
