namespace Repository.Models.Product
{
    public class CartOrdersRequest
    {
        public IEnumerable<CartOrderRequest>? CartOrderRequests { get; set; }
    }
}
