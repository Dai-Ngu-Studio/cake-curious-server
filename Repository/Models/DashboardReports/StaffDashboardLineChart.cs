namespace Repository.Models.DashboardReports
{
    public class StaffDashboardLineChart
    {
        public List<int> CurrentYearProcessedReports { get; set; } = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<int> CurrentYearUnprocessedReports { get; set; } = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public List<string> Month { get; set; } = new List<string> { "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12" };
    }
}