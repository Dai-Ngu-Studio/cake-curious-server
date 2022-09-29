using BusinessObject;
using Repository.Models.Product;
using Repository.Models.Recipes;

namespace Repository.Interfaces
{
    public interface IProductRepository
    {
        public Task Update(Product obj);
        public Task Add(Product obj);
        public Task<Product?> Delete(Guid id);
        public Task<Product?> GetById(Guid id);
        public IEnumerable<StoreDashboardProduct>? GetProducts(string? s, string? order_by, string? product_type, int pageIndex, int pageSize);
        public int CountDashboardProducts(string? s, string? order_by, string? product_type);
    }
}
