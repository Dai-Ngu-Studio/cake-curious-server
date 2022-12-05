using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Constants.Categories;
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
        public ActionResult<SimpleReportCategoryPage<SimpleReportCategory>> GetReportCategories(int la)
        {
            var reportCategoryPage = new SimpleReportCategoryPage<SimpleReportCategory>();
            reportCategoryPage.ReportCategories = (la == (int)CategoryLanguageEnum.English) 
                ? reportCategoryRepository.GetEnglishReportCategories() 
                : reportCategoryRepository.GetReportCategories();
            return Ok(reportCategoryPage);
        }
    }
}
