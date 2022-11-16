using BusinessObject;
using Repository.Models.Product;

namespace Repository.Interfaces
{
    public interface IProductRepository
    {
        public Task Update(Product obj);
        public Task Add(Product obj);
        public Task<Product?> Delete(Guid id);
        public Task<StoreProductDetail?> GetByIdForStore(Guid id);
        public Task<Product?> GetById(Guid id);
        public IEnumerable<StoreProductDetail>? GetProducts(string? s, string? order_by, string? product_type, int pageIndex, int pageSize);
        public int CountDashboardProducts(string? s, string? order_by, string? product_type);
        public Task<Product?> GetProductReadonly(Guid id);
        public Task<Product?> GetActiveProductReadonly(Guid id);
        public Task<ICollection<GroceryProduct>> Explore(int productType, int randSeed, int take, int key, Guid? storeId);
        public Task<DetailProduct?> GetProductDetails(Guid id);
        public Task<ICollection<GroceryProduct>> GetSuggestedProducts(List<Guid> productIds);
        public Task<ICollection<CartOrder>> GetCartOrders(List<Guid> storeIds, List<Guid> productIds);
    }
}
