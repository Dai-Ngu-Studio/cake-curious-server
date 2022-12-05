namespace Repository.Models.DashboardReports
{
    public class StaffDashboardCardStats
    {
        public int TodayReports { get; set; }
        public double SinceYesterdayReports { get; set; }
        public int CurrentWeekReportedRecipes { get; set; }
        public double SinceLastWeekReportedRecipes { get; set; }
        public int CurrentWeekReportedComments { get; set; }
        public double SinceLastWeekReportedComments { get; set; }
        public int CurrentWeekProcessedReports { get; set; }
        public double SinceLastWeekProcessedReports { get; set; }
    }
}