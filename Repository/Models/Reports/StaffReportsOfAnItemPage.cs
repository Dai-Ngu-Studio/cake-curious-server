

namespace Repository.Models.Reports
{
    public class StaffReportsOfAnItemPage
    {
        public IEnumerable<StaffDashboardReport>? Reports { get; set; }
        public int PendingReports { get; set; }
        public int TotalPage { get; set; }
    }
}
