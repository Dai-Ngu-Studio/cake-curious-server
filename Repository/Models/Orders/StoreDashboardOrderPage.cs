using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Orders
{
    public class StoreDashboardOrderPage
    {
        public int? TotalPage { get; set; }
        public IEnumerable<StoreDashboardOrder>? Orders { get; set; }
    }
}
