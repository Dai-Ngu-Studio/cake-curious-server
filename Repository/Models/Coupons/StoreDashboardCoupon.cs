using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Coupons
{
    public class StoreDashboardCoupon
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public decimal? Discount { get; set; }
        public int? DiscountType { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? MaxUses { get; set; }
        public int? Status { get; set; }

    }
}
