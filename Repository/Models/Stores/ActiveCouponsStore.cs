using Repository.Models.Coupons;

namespace Repository.Models.Stores
{
    public class ActiveCouponsStore
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? Rating { get; set; }
        public int? TotalCoupons { get; set; }
        public IEnumerable<UserAwareSimpleCoupon>? Coupons { get; set; }
        public int? Status { get; set; }
    }
}
