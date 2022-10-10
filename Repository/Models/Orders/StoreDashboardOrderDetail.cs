using Repository.Models.Coupons;
using Repository.Models.OrderDetails;
using Repository.Models.Stores;
using Repository.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Orders
{
    public class StoreDashboardOrderDetail
    {
        public Guid? Id { get; set; }
        public SimpleUser? User { get; set; }
        public IEnumerable<SimpleOrderDetail>? OrderDetails { get; set; }
        public decimal? Total { get; set; }
        public decimal? DiscountedTotal { get; set; }
        public int? Status { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public StoreOrderDetailCoupon? Coupon { get; set; }
    }
}
