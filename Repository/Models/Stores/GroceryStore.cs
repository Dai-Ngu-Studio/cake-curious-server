namespace Repository.Models.Stores
{
    public class GroceryStore
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? Rating { get; set; }
    }
}
