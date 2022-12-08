using Repository.Models.Product;

namespace Repository.Models.OrderDetails
{
    public class DetailOrderDetail
    {
        public Guid? Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public CartDetailProduct? Product { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Rating { get; set; }
    }
}
