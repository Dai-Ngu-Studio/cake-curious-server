using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.ProductCategories;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly IProductCategoryRepository _productCategoryRepository;

        public ProductCategoriesController(IProductCategoryRepository productCategoryRepository)
        {
            _productCategoryRepository = productCategoryRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ProductCategoryResponse>> GetProductCategories()
        {
            return Ok(await _productCategoryRepository.GetProductCategories());
        }

        [HttpGet("simple")]
        [Authorize]
        public ActionResult<SimpleProductCategories> GetSimpleProductCategories()
        {
            var productCategories = new SimpleProductCategories();
            productCategories.ProductCategories = _productCategoryRepository.GetSimpleProductCategories();
            return Ok(productCategories);
        }
    }
}
