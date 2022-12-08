namespace Repository.Models.DashboardReports
{
    public class StaffDashboardBarChart
    {
        public List<int> CurrentMonthReports { get; set; } = new List<int>() { 0, 0, 0, 0 };
        public List<int> LastMonthReports { get; set; } = new List<int>() { 0, 0, 0, 0 };
        public List<string> Week { get; set; } = new List<string> { "Tuần 1", "Tuần 2", "Tuần 3", "Tuần 4" };
    }
}