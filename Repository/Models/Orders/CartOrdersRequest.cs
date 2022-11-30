namespace Repository.Models.Orders
{
    public class CartOrdersRequest
    {
        public IEnumerable<CartOrderRequest>? CartOrderRequests { get; set; }
    }
}
