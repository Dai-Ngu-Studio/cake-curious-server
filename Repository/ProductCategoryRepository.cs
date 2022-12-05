using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.ProductCategories;

namespace Repository
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        public async Task<ProductCategoryResponse> GetProductCategories()
        {
            var db = new CakeCuriousDbContext();
            ProductCategoryResponse pcs = new ProductCategoryResponse();
            pcs.ProductCategories = await db.ProductCategories.ToListAsync();
            return pcs;
        }

        public IEnumerable<SimpleProductCategory> GetSimpleProductCategories()
        {
            var db = new CakeCuriousDbContext();
            return db.ProductCategories.ProjectToType<SimpleProductCategory>();
        }

        public IEnumerable<EngSimpleProductCategory> GetEnglishSimpleProductCategories()
        {
            var db = new CakeCuriousDbContext();
            return db.ProductCategories.ProjectToType<EngSimpleProductCategory>();
        }
    }
}
