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
        public IEnumerable<AdminDashboardStore>? GetStores(string? s, string? order_by, string? filter_Store, int PageSize, int PageIndex);
        public int CountDashboardStores(string? s, string? order_by, string? filter_Store);
        public Task<bool> IsStoreExisted(Guid id);
    }
}
