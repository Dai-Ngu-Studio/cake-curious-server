using BusinessObject;

namespace Repository.Interfaces
{
    public interface IUserDeviceRepository
    {
        public Task Add(UserDevice obj);
        public Task<UserDevice?> Get(string token);
        public IEnumerable<UserDevice> GetDevicesAfter(int take, string lastToken);
        public Task RemoveRange(List<string> tokens);
    }
}
