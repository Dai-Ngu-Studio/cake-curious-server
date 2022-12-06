
namespace Repository.Models.DashboardReports
{
    public class StaffDashboardStatistic
    {
        public StaffDashboardCardStats? CardStats { get; set; }
        public StaffDashboardBarChart? BarChart { get; set; }
        public StaffDashboardLineChart? LineChart { get; set; }
    }
}
