using Repository.Models.Coupons;
using Repository.Models.OrderDetails;
using Repository.Models.Stores;

namespace Repository.Models.Orders
{
    public class DetailOrder
    {
        public Guid? Id { get; set; }
        public string? Code { get; set; }
        public GroceryStore? Store { get; set; }
        public decimal? Total { get; set; }
        public decimal? DiscountedTotal { get; set; }
        public string? Address { get; set; }
        public int? Products { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public SimpleCoupon? Coupon { get; set; }
        public int? Status { get; set; }
        public IEnumerable<DetailOrderDetail>? OrderDetails { get; set; }
    }
}
