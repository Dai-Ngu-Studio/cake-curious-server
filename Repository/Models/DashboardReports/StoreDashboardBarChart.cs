namespace Repository.Models.DashboardReports
{
    public class StoreDashboardBarChart
    {
        public List<decimal>? CurrentMonthSales { get; set; } = new List<decimal>() { 0, 0, 0, 0 };
        public List<decimal>? LastMonthSales { get; set; } = new List<decimal>() { 0, 0, 0, 0 };
    }
}