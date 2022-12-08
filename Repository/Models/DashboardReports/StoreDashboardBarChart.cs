namespace Repository.Models.DashboardReports
{
    public class StoreDashboardBarChart
    {
        public List<decimal>? CurrentMonthSales { get; set; } = new List<decimal>() { 0, 0, 0, 0 };
        public List<decimal>? LastMonthSales { get; set; } = new List<decimal>() { 0, 0, 0, 0 };
        public List<string> Week { get; set; } = new List<string> { "Week1", "Week2", "Week3", "Week4" };

    }
}