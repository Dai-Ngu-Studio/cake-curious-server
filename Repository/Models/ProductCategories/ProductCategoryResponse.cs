using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.ProductCategories
{
    public class ProductCategoryResponse
    {
        public IEnumerable<ProductCategory>? ProductCategories { get; set; }
    }
}
