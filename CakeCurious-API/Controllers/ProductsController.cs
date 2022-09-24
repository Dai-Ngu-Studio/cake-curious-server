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
        public async Task<ActionResult<IEnumerable<Product>>> GetProduct(int PageSize,int PageIndex)
        {
           
                var result = await productRepository.GetProducts(PageSize, PageIndex);
                return Ok(result);        
        }
        [HttpGet("Search")]
        [Authorize]
        public ActionResult<IEnumerable<Product>> SearchProduct(string keyword)
        {
            return Ok(productRepository.SearchProduct(keyword));
        }
        [HttpGet("Filter")]
        [Authorize]
        public ActionResult<IEnumerable<Product>> FilterProduct(string keyword)
        {
            return Ok( productRepository.FilterProduct(keyword));
        }
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Product> PostProduct(Product product)
        {
            // if (_context.Products == null)
            // {
            //     return Problem("Entity set 'FStoreDBContext.Products'  is null.");
            // }
            try
            {
                productRepository.Add(product);
            }
            catch (DbUpdateException)
            {
                if (productRepository.GetById(product.Id.Value) != null)
                {
                    return Conflict();
                }

                throw;
            }

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        [HttpDelete("{guid}")]
        [Authorize]
        public async Task<ActionResult> DeleteProduct(Guid guid)
        {
            productRepository.Delete(guid);
            Product prod = await productRepository.GetById(guid);
            return Ok("Delete product "+prod.Name+" success");
        }
        [HttpPut("{guid}")]
        public async Task<ActionResult> PutProduct(Guid guid,Product product) {
            if (guid != product.Id)
            {
                return BadRequest();
            }

            try
            {
                 productRepository.Update(product);
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
