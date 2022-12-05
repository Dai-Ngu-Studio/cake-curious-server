namespace Repository.Models.ReportCategories
{
    public class SimpleReportCategoryPage<T>
    {
        public IEnumerable<T>? ReportCategories { get; set; }
    }
}
