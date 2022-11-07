using Microsoft.AspNetCore.Mvc;
using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repository.Models.Product;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Nest;
using Repository.Constants.Products;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository productRepository;
        private readonly IStoreRepository storeRepository;
        private readonly IElasticClient elasticClient;

        public ProductsController(IProductRepository _productRepository, IStoreRepository _storeRepository, IElasticClient _elasticClient)
        {
            productRepository = _productRepository;
            storeRepository = _storeRepository;
            elasticClient = _elasticClient;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<Product>> GetProducts(string? search, string? sort, string? filter, [Range(1, int.MaxValue)] int page = 1, [Range(1, int.MaxValue)] int size = 10)
        {
            var result = new StoreDashboardProductPage();
            result.Products = productRepository.GetProducts(search, sort, filter, page, size);
            result.TotalPage = (int)Math.Ceiling((decimal)productRepository.CountDashboardProducts(search, sort, filter) / size);
            return Ok(result);
        }

        [HttpGet("grocery/ingredients")]
        [Authorize]
        public async Task<GroceryProductPage> ExploreIngredients(int seed,
            [Range(1, int.MaxValue)] int take = 10,
            [Range(0, int.MaxValue)] int lastKey = 0)
        {
            var products = new GroceryProductPage();
            products.Products = await productRepository.Explore((int)ProductTypeEnum.Ingredient, seed, take, lastKey);
            return products;
        }

        [HttpGet("grocery/equipment")]
        [Authorize]
        public async Task<GroceryProductPage> ExploreEquipment(int seed,
            [Range(1, int.MaxValue)] int take = 10,
            [Range(0, int.MaxValue)] int lastKey = 0)
        {
            var products = new GroceryProductPage();
            products.Products = await productRepository.Explore((int)ProductTypeEnum.Tool, seed, take, lastKey);
            return products;
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<StoreProductDetail>> GetProductsById(Guid id)
        {
            var result = await productRepository.GetByIdForStore(id);
            return Ok(result);
        }

        [HttpGet("details/{id:guid}")]
        [Authorize]
        public async Task<ActionResult<DetailProduct>> GetRecipeDetails(Guid id)
        {
            try
            {
                // Get ID Token
                string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrWhiteSpace(uid))
                {
                    var product = await productRepository.GetProductDetails(id);
                    if (product != null)
                    {
                        return Ok(product);
                    }
                    return NotFound();
                }
                return Unauthorized();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid storeId = await storeRepository.getStoreIdByUid(uid!);
            if (storeId.ToString() == "00000000-0000-0000-0000-000000000000") return BadRequest("Invalid store ID. You need to create a store to create a product.");
            Guid id = Guid.NewGuid();
            Product prod = new Product()
            {
                Id = id,
                Name = product.Name,
                Description = product.Description,
                Discount = product.Discount / 100,
                PhotoUrl = product.PhotoUrl,
                Quantity = product.Quantity,
                StoreId = storeId,
                ProductCategoryId = product.ProductCategoryId,
                Price = product.Price,
                Status = product.Status,
                ProductType = product.ProductType,
            };

            try
            {
                await productRepository.Add(prod);

                var elasticsearchProduct = new ElasticsearchProduct
                {
                    Id = prod.Id,
                    Name = new string[] { prod.Name! },
                    Category = prod.ProductCategoryId,
                    Price = prod.Price,
                    Discount = prod.Discount,
                    StoreId = prod.StoreId,
                };

                var createResponse = await elasticClient.CreateAsync<ElasticsearchProduct>(elasticsearchProduct,
                    x => x
                        .Id(prod.Id)
                        .Index("products")
                    );
            }
            catch (DbUpdateException)
            {
                if (productRepository.GetById(prod.Id.Value) != null)
                    return Conflict();
            }
            return Ok(prod);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<ActionResult> HideProduct(Guid id)
        {
            Product? prod = await productRepository.Delete(id);
            await elasticClient.DeleteAsync<ElasticsearchProduct>(id);
            return Ok();
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<ActionResult> PutProduct(Guid id, Product product)
        {
            try
            {
                if (id != product.Id) return BadRequest();
                Product? beforeUpdateObj = await productRepository.GetById(product.Id.Value);
                if (beforeUpdateObj == null) return BadRequest("Product does not exist.");
                Product updateProd = new Product()
                {
                    Id = product.Id == null ? beforeUpdateObj.Id : product.Id,
                    Name = product.Name == null ? beforeUpdateObj.Name : product.Name,
                    Description = product.Description == null ? beforeUpdateObj.Description : product.Description,
                    Discount = product.Discount == null ? beforeUpdateObj.Discount : product.Discount / 100,
                    PhotoUrl = product.PhotoUrl == null ? beforeUpdateObj.PhotoUrl : product.PhotoUrl,
                    Quantity = product.Quantity == null ? beforeUpdateObj.Quantity : product.Quantity,
                    Price = product.Price == 0 ? beforeUpdateObj.Price : product.Price,
                    ProductType = product.ProductType == null ? beforeUpdateObj.ProductType : product.ProductType,
                    Status = product.Status == null ? beforeUpdateObj.Status : product.Status,
                    StoreId = product.StoreId == null ? beforeUpdateObj.StoreId : product.StoreId,
                    ProductCategoryId = product.ProductCategoryId == null ? beforeUpdateObj.ProductCategoryId : product.ProductCategoryId,
                };
                await productRepository.Update(updateProd);

                var elasticsearchProduct = new ElasticsearchProduct
                {
                    Id = updateProd.Id,
                    Name = new string[] { updateProd.Name! },
                    Category = updateProd.ProductCategoryId,
                    Price = updateProd.Price,
                    Discount = updateProd.Discount,
                    StoreId = updateProd.StoreId,
                };

                var updateResponse = await elasticClient.UpdateAsync<ElasticsearchProduct>(updateProd.Id,
                    x => x
                        .Index("products")
                        .Doc(elasticsearchProduct)
                    );
            }
            catch (DbUpdateConcurrencyException)
            {
                if (productRepository.GetById(id) == null)
                {
                    return NotFound();
                }

                throw;
            }
            return NoContent();
        }

    }
}
