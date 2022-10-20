using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.ProductCategories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
