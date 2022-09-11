using BusinessObject;

namespace Repository.Interfaces
{
    public interface IUserRepository
    {
        public IQueryable<User> GetList();
        public Task<User?> Get(string uid);
        public Task Add(User obj);
        public Task Update(User obj);
    }
}
