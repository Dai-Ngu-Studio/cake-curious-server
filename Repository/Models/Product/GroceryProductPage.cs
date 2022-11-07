namespace Repository.Models.Product
{
    public class GroceryProductPage
    {
        public int? TotalPages { get; set; }
        public IEnumerable<GroceryProduct>? Products { get; set; }
    }
}
