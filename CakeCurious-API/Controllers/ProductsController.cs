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
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(string? search, string? sort, string? filter, [Range(1, int.MaxValue)] int page = 1, [Range(1, int.MaxValue)] int size = 10)
        {
            var result = new StoreDashboardProductPage();
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid storeId = await storeRepository.getStoreIdByUid(uid!);
            if (storeId.ToString() == "00000000-0000-0000-0000-000000000000") return BadRequest("Invalid store ID. You need to create a store to get product list.");
            result.Products = productRepository.GetProducts(storeId, search, sort, filter, page, size);
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
            products.Products = await productRepository.Explore((int)ProductTypeEnum.Ingredient, seed, take, lastKey, null);
            return products;
        }

        [HttpGet("grocery/equipment")]
        [Authorize]
        public async Task<GroceryProductPage> ExploreEquipment(int seed,
            [Range(1, int.MaxValue)] int take = 10,
            [Range(0, int.MaxValue)] int lastKey = 0)
        {
            var products = new GroceryProductPage();
            products.Products = await productRepository.Explore((int)ProductTypeEnum.Tool, seed, take, lastKey, null);
            return products;
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<StoreProductDetail>> GetProductsById(Guid id)
        {
            var result = await productRepository.GetByIdForStore(id);
            return Ok(result);
        }

        [HttpGet("{id:guid}/details")]
        [Authorize]
        public async Task<ActionResult<DetailProduct>> GetProductDetails(Guid id)
        {
            var product = await productRepository.GetProductDetails(id);
            if (product != null)
            {
                return Ok(product);
            }
            return NotFound();
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
                    return Conflict("This product already exist.");
            }
            return Ok(prod);
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<ActionResult> HideProduct(Guid id)
        {
            Product? prod = await productRepository.Delete(id);
            await elasticClient.DeleteAsync(new DeleteRequest(index: "products", prod!.Id));
            return Ok("Delete product " + prod.Name + "  successfully");
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
                    Price = product.Price == null ? beforeUpdateObj.Price : product.Price,
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

                // Does doc exist on Elasticsearch?
                var existsResponse = await elasticClient.DocumentExistsAsync(new DocumentExistsRequest(index: "products", updateProd.Id));
                if (!existsResponse.Exists)
                {
                    // Doc doesn't exist, create new
                    var createResponse = await elasticClient.CreateAsync<ElasticsearchProduct>(elasticsearchProduct,
                        x => x
                            .Id(updateProd.Id)
                            .Index("products")
                        );
                }
                else
                {
                    // Doc exists, update
                    var updateResponse = await elasticClient.UpdateAsync<ElasticsearchProduct>(updateProd.Id,
                        x => x
                            .Index("products")
                            .Doc(elasticsearchProduct)
                        );
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (productRepository.GetById(id) == null)
                {
                    return NotFound();
                }

                throw;
            }
            return Ok("Update product successfully");
        }

        [HttpPost("cart")]
        [Authorize]
        public async Task<ActionResult<CartOrders>> GetCartOrdersRequest(CartOrdersRequest ordersRequest)
        {
            var cartOrdersRequests = ordersRequest.CartOrderRequests;
            if (cartOrdersRequests != null)
            {
                List<Guid> storeIds = cartOrdersRequests.Select(x => (Guid)x.StoreId!).ToList();
                List<Guid> productIds = cartOrdersRequests.SelectMany(x => x.ProductIds ?? Enumerable.Empty<Guid>()).ToList();
                var cartOrders = await productRepository.GetCartOrders(storeIds, productIds);
                var orders = new CartOrders
                {
                    Orders = storeIds.Join(cartOrders, id => id, od => (Guid)od.Store!.Id!, (id, od) => od),
                };

                foreach (var order in orders.Orders)
                {
                    var orderProductIds = cartOrdersRequests
                        .Where(x => x.StoreId == order.Store!.Id)
                        .SelectMany(x => x.ProductIds ?? Enumerable.Empty<Guid>());
                    order.Products = orderProductIds.Join(order.Products!, id => id, pd => (Guid)pd.Id!, (id, pd) => pd);
                }
                return Ok(orders);
            }
            return BadRequest();
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<GroceryProductPage>> SearchProducts(
            [FromQuery] string? query,
            [FromQuery] int[] categories,
            [FromQuery] Guid? storeId,
            [FromQuery] decimal? fromPrice,
            [FromQuery] decimal? toPrice,
            [FromQuery] Guid? lastId,
            [FromQuery] double? lastScore,
            [Range(1, int.MaxValue)] int take = 5)
        {
            var searchDescriptor = new SearchDescriptor<ElasticsearchProduct>();
            var descriptor = new QueryContainerDescriptor<ElasticsearchProduct>();
            var shouldContainer = new List<QueryContainer>();
            var filterContainer = new List<QueryContainer>();

            if (string.IsNullOrWhiteSpace(query)
                && (categories == null || categories.Length == 0)
                && storeId == null && fromPrice == null && toPrice == null)
            {
                var emptyPage = new GroceryProductPage();
                emptyPage.TotalPages = 0;
                emptyPage.Products = new List<GroceryProduct>();

                return Ok(emptyPage);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                shouldContainer.Add(descriptor
                    .Match(m => m
                        .Field(f => f.Name)
                        .Query(query)
                        .Fuzziness(Fuzziness.EditDistance(2))
                    )
                );
            }

            if (categories != null)
            {
                shouldContainer.Add(descriptor
                    .Terms(x => x
                        .Field(f => f.Category)
                        .Terms(categories)
                    )
                );

                filterContainer.Add(descriptor
                    .Terms(x => x
                        .Field(f => f.Category)
                        .Terms(categories)
                    )
                );
            }

            if (storeId != null)
            {
                shouldContainer.Add(descriptor
                    .Terms(x => x
                        .Field(f => f.StoreId)
                        .Terms(storeId)
                    )
                );

                filterContainer.Add(descriptor
                    .Terms(x => x
                        .Field(f => f.StoreId)
                        .Terms(storeId)
                    )
                );
            }

            if (fromPrice != null)
            {
                shouldContainer.Add(descriptor
                    .Range(x => x
                        .Field("discountPrice")
                        .GreaterThanOrEquals(decimal.ToDouble((decimal)fromPrice))
                    )
                );

                filterContainer.Add(descriptor
                    .Range(x => x
                        .Field("discountPrice")
                        .GreaterThanOrEquals(decimal.ToDouble((decimal)fromPrice))
                    )
                );
            }

            if (toPrice != null)
            {
                shouldContainer.Add(descriptor
                    .Range(x => x
                        .Field("discountPrice")
                        .LessThanOrEquals(decimal.ToDouble((decimal)toPrice))
                    )
                );

                filterContainer.Add(descriptor
                    .Range(x => x
                        .Field("discountPrice")
                        .LessThanOrEquals(decimal.ToDouble((decimal)toPrice))
                    )
                );
            }

            if (lastId != null && lastScore != null)
            {
                searchDescriptor = searchDescriptor.SearchAfter(lastScore, lastId);
            }

            searchDescriptor = searchDescriptor.Index("products")
                .Size(take)
                .MinScore(0.01D)
                .Sort(ss => ss
                    .Descending(SortSpecialField.Score)
                    .Descending(f => f.Id.Suffix("keyword"))
                )
                .Query(q => q
                    .Bool(b => b
                        .Should(shouldContainer.ToArray())
                        .Filter(filterContainer.ToArray())
                    )
                );

            var searchResponse = await elasticClient.SearchAsync<ElasticsearchProduct>(searchDescriptor);

            var elasticsearchProducts = new List<KeyValuePair<Guid, double>>();

            foreach (var hit in searchResponse.Hits)
            {
                elasticsearchProducts.Add(
                    new KeyValuePair<Guid, double>((Guid)hit.Source.Id!, (double)hit.Score!)
                );
            }

            var productIds = elasticsearchProducts.Select(x => x.Key).ToList();
            var suggestedProducts = await productRepository.GetSuggestedProducts(productIds);

            var products = elasticsearchProducts
                .Join(suggestedProducts, es => es.Key, pd => (Guid)pd.Id!,
                    (es, pd) => { pd.Score = es.Value; return pd; });

            var productPage = new GroceryProductPage();
            productPage.Products = products;

            return Ok(productPage);
        }
    }
}
