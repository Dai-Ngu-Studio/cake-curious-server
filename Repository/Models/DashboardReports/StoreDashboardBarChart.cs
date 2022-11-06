namespace Repository.Models.DashboardReports
{
    public class StoreDashboardBarChart
    {
        public List<decimal>? CurrentWeekSales { get; set; } = new List<decimal>() { 0, 0, 0, 0 };
        public List<decimal>? LastWeekSales { get; set; } = new List<decimal>() { 0, 0, 0, 0 };
    }
}