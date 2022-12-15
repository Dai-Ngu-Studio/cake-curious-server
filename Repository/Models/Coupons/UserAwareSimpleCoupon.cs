namespace Repository.Models.Coupons
{
    public class UserAwareSimpleCoupon
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public decimal? Discount { get; set; }
        public int? DiscountType { get; set; }
        public int? MaxUses { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? UsedCount { get; set; }
        public bool? IsUsedByCurrentUser { get; set; }
        public int? Status { get; set; }
    }
}
