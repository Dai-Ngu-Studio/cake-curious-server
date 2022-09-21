using BusinessObject;

namespace Repository.Interfaces
{
    public interface IUserDeviceRepository
    {
        public IQueryable<UserDevice> GetList();
        public Task<UserDevice?> Get(string token);
        public Task Add(UserDevice obj);
    }
}
