using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        public async Task<IEnumerable<ProductCategory>> GetProductCategories()
        {
            var db = new CakeCuriousDbContext();
            return await db.ProductCategories.ToListAsync();
        }
    }
}
