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

        public IEnumerable<UserDevice> GetDevicesAfter(int take, string lastToken)
        {
            var db = new CakeCuriousDbContext();
            return db.UserDevices.OrderBy(x => x.Token).Where(x => x.Token!.CompareTo(lastToken) > 0).Take(take);
        }

        public async Task RemoveRange(List<string> tokens)
        {
            var db = new CakeCuriousDbContext();
            var devices = db.UserDevices.Where(x => tokens.Any(y => y == x.Token));
            db.UserDevices.RemoveRange(devices);
            await db.SaveChangesAsync();
        }
    }
}
