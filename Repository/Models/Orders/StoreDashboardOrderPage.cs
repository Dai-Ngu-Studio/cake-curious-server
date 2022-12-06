
namespace Repository.Models.Orders
{
    public class StoreDashboardOrderPage
    {
        public int? TotalPage { get; set; }
        public IEnumerable<StoreDashboardOrder>? Orders { get; set; }
    }
}
