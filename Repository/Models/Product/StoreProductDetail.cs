using Repository.Models.ProductCategories;

namespace Repository.Models.Product
{
    public class StoreProductDetail
    {
        public Guid? Id { get; set; }
        public int? ProductType { get; set; }
        public SimpleProductCategory? ProductCategory { get; set; }
        public int? ProductCategoryId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? Rating { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int? Status { get; set; }
    }
}
