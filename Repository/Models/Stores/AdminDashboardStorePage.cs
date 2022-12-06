

namespace Repository.Models.Stores
{
    public class AdminDashboardStorePage
    {
        public int? TotalPage { get; set; }
        public IEnumerable<AdminDashboardStore>? Stores { get; set; }
    }
}
