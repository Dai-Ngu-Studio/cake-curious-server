using Repository.Models.OrderDetails;
using Repository.Models.Users;

namespace Repository.Models.Orders
{
    public class StoreDashboardOrder
    {
        public Guid? Id { get; set; }
        public SimpleUser? User { get; set; }
        public IEnumerable<SimpleOrderDetail>? OrderDetails { get; set; }
        public string? Address { get; set; }
        public decimal? DiscountedTotal { get; set; }
        public int? Status { get; set; }
        public DateTime? OrderDate { get; set; }

    }
}
