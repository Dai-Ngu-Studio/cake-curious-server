namespace Repository.Models.Product
{
    public class GroceryProduct
    {
        public Guid? Id { get; set; }
        public int? ProductType { get; set; }
        public string? Name { get; set; }
        public decimal? Price { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? Rating { get; set; }
        public int? Key { get; set; }
        public double? Score { get; set; }
    }
}
