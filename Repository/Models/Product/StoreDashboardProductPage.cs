using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Product
{
    public class StoreDashboardProductPage
    {
        public int? TotalPage { get; set; }
        public IEnumerable<StoreDashboardProduct>? Products { get; set; }
    }
}
