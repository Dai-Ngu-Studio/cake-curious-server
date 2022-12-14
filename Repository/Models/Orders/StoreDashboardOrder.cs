using Repository.Models.Users;

namespace Repository.Models.Orders
{
    public class StoreDashboardOrder
    {
        public Guid? Id { get; set; }
        public string? Code { get; set; }
        public SimpleUser? User { get; set; }
        public string? Address { get; set; }
        public decimal? DiscountedTotal { get; set; }
        public decimal? Total { get; set; }
        public int? Status { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
