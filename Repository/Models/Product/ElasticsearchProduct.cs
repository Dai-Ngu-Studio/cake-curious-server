namespace Repository.Models.Product
{
    public class ElasticsearchProduct
    {
        public Guid? Id { get; set; }
        public string[]? Name { get; set; }
        public int? Category { get; set; }
        public decimal? Price { get; set; }
        public Guid? StoreId { get; set; }
        public string? Ingredient { get; set; }
    }
}
