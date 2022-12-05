using Repository.Models.ProductCategories;

namespace Repository.Interfaces
{
    public interface IProductCategoryRepository
    {
        public Task<ProductCategoryResponse> GetProductCategories();
        public IEnumerable<SimpleProductCategory> GetSimpleProductCategories();
        public IEnumerable<EngSimpleProductCategory> GetEnglishSimpleProductCategories();
    }
}
