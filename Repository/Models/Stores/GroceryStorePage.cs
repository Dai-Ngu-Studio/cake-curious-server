namespace Repository.Models.Stores
{
    public class GroceryStorePage
    {
        public int? TotalPages { get; set; }
        public IEnumerable<GroceryStore>? Stores { get; set; }
    }
}
