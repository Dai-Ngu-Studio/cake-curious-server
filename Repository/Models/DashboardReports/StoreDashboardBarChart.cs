namespace Repository.Models.DashboardReports
{
    public class StoreDashboardBarChart
    {
        public List<decimal>? CurrentWeekSales { get; set; } = new List<decimal>();
        public List<decimal>? LastWeekSales { get; set; } = new List<decimal>();
    }
}