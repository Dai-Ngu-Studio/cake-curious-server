using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Constants.Categories;
using Repository.Interfaces;
using Repository.Models.ProductCategories;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly IProductCategoryRepository productCategoryRepository;

        public ProductCategoriesController(IProductCategoryRepository _productCategoryRepository)
        {
            productCategoryRepository = _productCategoryRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ProductCategoryResponse>> GetProductCategories()
        {
            return Ok(await productCategoryRepository.GetProductCategories());
        }

        [HttpGet("simple")]
        [Authorize]
        public ActionResult<SimpleProductCategories<SimpleProductCategory>> GetSimpleProductCategories(int la)
        {
            var productCategories = new SimpleProductCategories<SimpleProductCategory>();
            productCategories.ProductCategories = (la == (int)CategoryLanguageEnum.English) 
                ? productCategoryRepository.GetEnglishSimpleProductCategories() 
                : productCategoryRepository.GetSimpleProductCategories();
            return Ok(productCategories);
        }
    }
}
