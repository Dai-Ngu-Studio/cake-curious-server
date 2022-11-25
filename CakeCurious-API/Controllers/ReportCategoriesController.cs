using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.ReportCategories;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportCategoriesController : ControllerBase
    {
        private readonly IReportCategoryRepository reportCategoryRepository;

        public ReportCategoriesController(IReportCategoryRepository _reportCategoryRepository)
        {
            reportCategoryRepository = _reportCategoryRepository;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<SimpleReportCategoryPage> GetReportCategories()
        {
            var reportCategoryPage = new SimpleReportCategoryPage();
            reportCategoryPage.ReportCategories = reportCategoryRepository.GetReportCategories();
            return Ok(reportCategoryPage);
        }
    }
}
