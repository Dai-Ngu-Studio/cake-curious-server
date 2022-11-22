using Repository.Models.Stores;

namespace Repository.Models.Orders
{
    public class InfoOrder
    {
        public Guid? Id { get; set; }
        public GroceryStore? Store { get; set; }
        public decimal? Total { get; set; }
        public decimal? DiscountedTotal { get; set; }
        public int? Products { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int? Status { get; set; }
    }
}
