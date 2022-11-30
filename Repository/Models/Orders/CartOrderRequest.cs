namespace Repository.Models.Orders
{
    public class CartOrderRequest
    {
        public Guid? StoreId { get; set; }
        public IEnumerable<Guid>? ProductIds { get; set; }
        public Guid? CouponId { get; set; }
    }
}
