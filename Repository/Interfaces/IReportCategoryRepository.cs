using Repository.Models.ReportCategories;

namespace Repository.Interfaces
{
    public interface IReportCategoryRepository
    {
        public IEnumerable<SimpleReportCategory> GetReportCategories();
    }
}
