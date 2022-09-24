using BusinessObject;
using Repository.Models.Recipes;

namespace Repository.Interfaces
{
    public interface IProductRepository
    {
        public Task<IEnumerable<Product>> GetProducts(int PageSize, int? PageIndex);
        public Task Update(Product obj);
        public Task Add(Product obj);
        public Task Delete(Guid id);
        public Task<Product> GetById(Guid id);
        public Task<IEnumerable<Product>> FilterProduct(string keyWord);
        public Task<IEnumerable<Product>> SearchProduct(string keyWord);
    }
}
