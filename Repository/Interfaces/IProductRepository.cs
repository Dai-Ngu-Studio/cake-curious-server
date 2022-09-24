using BusinessObject;
using Repository.Models.Recipes;

namespace Repository.Interfaces
{
    public interface IProductRepository
    {
        public Task<IEnumerable<Product>> GetProducts(int PageSize, int PageIndex);
        public Task Update(Product obj);
        public Task Add(Product obj);
        public Task<Product> Delete(Guid id);
        public Task<Product> GetById(Guid id);
        public IEnumerable<Product> FindProduct(string s,string filter_product);
    }
}
