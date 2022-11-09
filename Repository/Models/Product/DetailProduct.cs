using Repository.Models.ProductCategories;
using Repository.Models.Stores;

namespace Repository.Models.Product
{
    public class DetailProduct
    {
        public Guid? Id { get; set; }
        public GroceryStore? Store { get; set; }
        public int? ProductType { get; set; }
        public SimpleProductCategory? ProductCategory { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public string? PhotoUrl { get; set; }
        public int? Status { get; set; }
    }
}
