using BusinessObject;

namespace Repository.Interfaces
{
    public interface IUserDeviceRepository
    {
        public Task Add(UserDevice obj);
        public Task<UserDevice?> GetReadonly(string token);
        public IEnumerable<UserDevice> GetDevicesReadonlyAfter(int take, string lastToken);
        public IEnumerable<UserDevice> GetDevicesOfUserReadonly(string uid);
        public Task<List<string>> GetDeviceTokensOfUsersReadonly(IEnumerable<string> userIds);
        public Task RemoveRange(List<string> tokens);
    }
}
