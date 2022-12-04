namespace Repository.Models.Product
{
    public class BundleDetailProduct
    {
        public Guid? Id { get; set; }
        public Guid? StoreId { get; set; }
        public string? Name { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? Rating { get; set; }
        public int? RatingCount { get; set; }
        public int? Status { get; set; }
        public string? Ingredient { get; set; }
    }
}
