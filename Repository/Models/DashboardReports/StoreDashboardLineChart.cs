namespace Repository.Models.DashboardReports
{
    public class StoreDashboardLineChart
    {
            public List<int> CurrentYearStoreVisit { get; set; } = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            public List<int> LastYearStoreVisit { get; set; } = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    }
}