namespace Repository.Models.DashboardReports
{
    public class StoreDashboardBarChart
    {
        public List<decimal>? CurrentMonthSales { get; set; } = new List<decimal>() { 0, 0, 0, 0 };
        public List<decimal>? LastMonthSales { get; set; } = new List<decimal>() { 0, 0, 0, 0 };
        public List<string> Week { get; set; } = new List<string> { "Tuần 1", "Tuần 2", "Tuần 3", "Tuần 4" };

    }
}