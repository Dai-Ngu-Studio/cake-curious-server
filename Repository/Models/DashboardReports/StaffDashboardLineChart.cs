namespace Repository.Models.DashboardReports
{
    public class StaffDashboardLineChart
    {
        public List<int> CurrentYearProcessedReports { get; set; } = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<int> CurrentYearUnprocessedReports { get; set; } = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    }
}