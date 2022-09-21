using BusinessObject;

namespace Repository.Interfaces
{
    public interface IUserRepository
    {
        public ICollection<User> GetList();
        public Task<User?> Get(string uid);
        public Task Add(User obj);
        public Task Update(User obj);
    }
}
