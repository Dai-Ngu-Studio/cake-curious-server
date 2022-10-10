using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Coupons
{
    public class StoreDashboardCouponPage
    {
        public int? TotalPage { get; set; }
        public IEnumerable<StoreDashboardCoupon>? Coupons { get; set; }
    }
}
