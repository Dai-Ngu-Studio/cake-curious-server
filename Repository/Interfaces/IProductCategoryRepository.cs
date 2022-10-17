using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IProductCategoryRepository
    {
        public Task<IEnumerable<ProductCategory>> GetProductCategories();
    }
}
