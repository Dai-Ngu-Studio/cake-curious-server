using BusinessObject;
using Repository.Models.Stores;

namespace Repository.Interfaces
{
    public interface IStoreRepository
    {
        public Task Update(Store obj);
        public Task Add(Store obj);
        public Task<Store?> Delete(Guid? id);
        public Task<Store?> GetById(Guid id);
        public Task<StoreDetail?> GetByUserId(string? id);
        public Task<AdminDashboardStore?> GetStoreDetailForWeb(Guid guid);
        public IEnumerable<AdminDashboardStore>? GetStores(string? s, string? order_by, string? filter_Store, int PageSize, int PageIndex);
        public int CountDashboardStores(string? s, string? filter_Store);
        public Task<CartStore?> GetReadonlyCartStore(Guid id);
        public Task<Guid> getStoreIdByUid(string uid);
        public Task<DetailStore?> GetStoreDetails(Guid id);
        public Task<Store> CreateStoreForUser(User user, Store store);
        public Task<ICollection<GroceryStore>> Explore(int randSeed, int take, int key);
        public Task<ICollection<GroceryStore>> GetSuggestedStores(List<Guid> storeIds);
    }
}
