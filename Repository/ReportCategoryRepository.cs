using BusinessObject;
using Mapster;
using Repository.Interfaces;
using Repository.Models.ReportCategories;

namespace Repository
{
    public class ReportCategoryRepository : IReportCategoryRepository
    {
        public IEnumerable<SimpleReportCategory> GetReportCategories()
        {
            var db = new CakeCuriousDbContext();
            return db.ReportCategories.ProjectToType<SimpleReportCategory>();
        }
    }
}
