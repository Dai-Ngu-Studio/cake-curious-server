using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Repository.Interfaces;
using System.Net.Mime;
using Microsoft.EntityFrameworkCore;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository productRepository;

        public ProductsController(IProductRepository _productRepository)
        {
            productRepository = _productRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(int PageSize, int PageIndex)
        {
            var result = await productRepository.GetProducts(PageSize, PageIndex);
            return Ok(result);
        }

        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<Product>> GetProductsById(Guid guid)
        {
            var result = await productRepository.GetById(guid);
            return Ok(result);
        }

        [HttpGet("Find")]
        [Authorize]
        public ActionResult<IEnumerable<Product>> FindProduct(string s,string filter_product)
        {
            return Ok(productRepository.FindProduct(s,filter_product));
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> PostProduct(Product product)
        {
            Guid id = Guid.NewGuid();
            Product prod  = new Product(){
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
                productRepository.Add(prod);
            }
            catch (DbUpdateException)
            {
                if (productRepository.GetById(prod.Id.Value) != null) 
                    return Conflict();
            }
            //return CreatedAtAction("GetProductsById", new { id = prod.Id }, prod);
            return Ok(prod);
        }

        [HttpDelete("{guid}")]
        [Authorize]
        public async Task<ActionResult> DeleteProduct(Guid guid)
        {
            Product prod = await productRepository.Delete(guid);
            return Ok("Delete product " + prod.Name + " success");
        }

        [HttpPut("{guid}")]
        public async Task<ActionResult> PutProduct(Guid guid, Product product)
        {          
            try
            {
                if (guid != product.Id) return BadRequest();
                Product beforeUpdateProd = await productRepository.GetById(product.Id.Value);
                if (beforeUpdateProd == null) throw new Exception("Product that need to update does not exist");
                Product updateProd = new Product()
                {
                    Id = product.Id == null ? beforeUpdateProd.Id : product.Id ,

                    Name = product.Name == null ? beforeUpdateProd.Name : product.Name,

                    Description = product.Description == null ? beforeUpdateProd.Description : product.Description,

                    Discount = product.Discount == 0 ? beforeUpdateProd.Discount : product.Discount,

                    PhotoUrl = product.PhotoUrl == null ? beforeUpdateProd.PhotoUrl : product.PhotoUrl,

                    Quantity = product.Quantity == 0 ? beforeUpdateProd.Quantity : product.Quantity,

                    Price = product.Price == 0 ? beforeUpdateProd.Price : product.Price,

                    ProductType = product.ProductType == null ? beforeUpdateProd.ProductType : product.ProductType,

                    Status = product.Status == null ? beforeUpdateProd.Status : product.Status,

                    StoreId = product.StoreId == null ? beforeUpdateProd.StoreId : product.StoreId,

                    ProductCategoryId = product.ProductCategoryId == null ? beforeUpdateProd.ProductCategoryId : product.ProductCategoryId,
                };
                await productRepository.Update(updateProd);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (productRepository.GetById(guid) == null)
                {
                    return NotFound();
                }

                throw;
            }
            return NoContent();
        }

    }
}
