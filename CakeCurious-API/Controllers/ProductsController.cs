using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Repository.Interfaces;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;
using Repository.Models.Product;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<Product>> GetProducts(string? s, string? filter_Product, int PageIndex, int PageSize)
        {
            var result = new StoreDashboardProductPage();
            result.Products = _productRepository.GetProducts(s, filter_Product, PageIndex, PageSize);
            result.TotalPage = (int)Math.Ceiling((decimal)_productRepository.CountDashboardProducts(s!, filter_Product!) / PageSize);
            return Ok(result);
        }

        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<Product>> GetProductsById(Guid guid)
        {
            var result = await _productRepository.GetById(guid);
            return Ok(result);
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> PostProduct(Product product)
        {
            Guid id = Guid.NewGuid();
            Product prod = new Product()
            {
                Id = id,
                Name = product.Name,
                Description = product.Description,
                Discount = product.Discount,
                PhotoUrl = product.PhotoUrl,
                Quantity = product.Quantity,
                StoreId = product.StoreId,
                ProductCategoryId = product.ProductCategoryId,
                Price = product.Price,
                Status = product.Status,
                ProductType = product.ProductType,

            };
            try
            {
                _productRepository.Add(prod);
            }
            catch (DbUpdateException)
            {
                if (_productRepository.GetById(prod.Id.Value) != null)
                    return Conflict();
            }
            return Ok(prod);
        }

        [HttpPut("hide/{guid}")]
        [Authorize]
        public async Task<ActionResult> HideProduct(Guid guid)
        {
            Product? prod = await _productRepository.HideProduct(guid);
            return Ok("Delete product " + prod!.Name + " success");
        }

        [HttpPut("{guid}")]
        public async Task<ActionResult> PutProduct(Guid guid, Product product)
        {
            try
            {
                if (guid != product.Id) return BadRequest();
                Product? beforeUpdateObj = await _productRepository.GetById(product.Id.Value);
                if (beforeUpdateObj == null) throw new Exception("Product that need to update does not exist");
                Product updateProd = new Product()
                {
                    Id = product.Id == null ? beforeUpdateObj.Id : product.Id,
                    Name = product.Name == null ? beforeUpdateObj.Name : product.Name,
                    Description = product.Description == null ? beforeUpdateObj.Description : product.Description,
                    Discount = product.Discount == 0 ? beforeUpdateObj.Discount : product.Discount,
                    PhotoUrl = product.PhotoUrl == null ? beforeUpdateObj.PhotoUrl : product.PhotoUrl,
                    Quantity = product.Quantity == 0 ? beforeUpdateObj.Quantity : product.Quantity,
                    Price = product.Price == 0 ? beforeUpdateObj.Price : product.Price,
                    ProductType = product.ProductType == null ? beforeUpdateObj.ProductType : product.ProductType,
                    Status = product.Status == null ? beforeUpdateObj.Status : product.Status,
                    StoreId = product.StoreId == null ? beforeUpdateObj.StoreId : product.StoreId,
                    ProductCategoryId = product.ProductCategoryId == null ? beforeUpdateObj.ProductCategoryId : product.ProductCategoryId,
                };
                await _productRepository.Update(updateProd);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_productRepository.GetById(guid) == null)
                {
                    return NotFound();
                }

                throw;
            }
            return NoContent();
        }

    }
}
