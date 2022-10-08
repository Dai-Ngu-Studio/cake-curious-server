namespace Repository.Models.Coupons
{
    public class SimpleCoupon
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public decimal? Discount { get; set; }
        public int? DiscountType { get; set; }
        public int? MaxUses { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? UsedCount { get; set; }
    }
}
