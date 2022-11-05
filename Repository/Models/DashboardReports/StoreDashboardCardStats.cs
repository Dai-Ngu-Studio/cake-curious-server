namespace Repository.Models.DashboardReports
{
    public class StoreDashboardCardStats
    {
        public decimal CurrentMonthProductSold{ get; set; }
        public double SinceLastMonthProductSold { get; set; }
        public decimal CurrentWeekSales{ get; set; }
        public double SinceLastWeekSales { get; set; }
        public decimal CurrentMonthTotalCompletedOrder { get; set; }
        public double SinceLastMonthTotalCompletedOrder { get; set; }
        public int CurrentWeekStoreVisit { get; set; }
        public double SinceLastWeekStoreVisit { get; set; }
    }
}