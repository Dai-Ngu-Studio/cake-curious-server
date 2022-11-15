using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using Repository.Constants.Products;
using Repository.Interfaces;
using Repository.Models.Coupons;
using Repository.Models.Product;
using Repository.Models.Stores;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly IStoreRepository storeRepository;
        private readonly ICouponRepository couponRepository;
        private readonly IOrderRepository orderRepository;
        private readonly IProductRepository productRepository;
        private readonly IElasticClient elasticClient;

        public StoresController(IStoreRepository _storeRepository, ICouponRepository _couponRepository, IOrderRepository _orderRepository, IProductRepository _productRepository, IElasticClient _elasticClient)
        {
            storeRepository = _storeRepository;
            couponRepository = _couponRepository;
            orderRepository = _orderRepository;
            productRepository = _productRepository;
            elasticClient = _elasticClient;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<AdminDashboardStorePage>> GetStores(string? search, string? sort, string? filter, [Range(1, int.MaxValue)] int size = 10, [Range(1, int.MaxValue)] int page = 1)
        {
            var result = new AdminDashboardStorePage();
            result.Stores = storeRepository.GetStores(search, sort, filter, size, page);
            result.TotalPage = (int)Math.Ceiling((decimal)storeRepository.CountDashboardStores(search, sort, filter) / size);
            return Ok(result);
        }

        [HttpGet("Of-A-User")]
        [Authorize]
        public async Task<ActionResult<StoreDetail>> GetStoresByUserId()
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            StoreDetail? result = await storeRepository.GetByUserId(uid);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<Store>> GetStoresById(Guid id)
        {
            var result = await storeRepository.GetStoreDetailForWeb(id);
            return Ok(result);
        }

        [HttpGet("{id:guid}/details")]
        [Authorize]
        public async Task<ActionResult<DetailStore>> GetStoreDetails(Guid id)
        {
            var store = await storeRepository.GetStoreDetails(id);
            if (store != null)
            {
                return Ok(store);
            }
            return NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Store>> PostStore(Store store)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                try
                {
                    Guid id = Guid.NewGuid();
                    var newStore = new Store
                    {
                        Address = store.Address,
                        Description = store.Description,
                        Id = id,
                        PhotoUrl = store.PhotoUrl,
                        Name = store.Name,
                        UserId = uid,
                        Rating = store.Rating,
                        Status = store.Status,
                    };

                    await storeRepository.Add(newStore);

                    var elasticsearchStore = new ElasticsearchStore
                    {
                        Id = newStore.Id,
                        Name = newStore.Name,
                        Rating = newStore.Rating,
                    };

                    var createResponse = await elasticClient.CreateAsync<ElasticsearchStore>(elasticsearchStore,
                        x => x
                            .Id(newStore.Id)
                            .Index("stores")
                        );

                    return Ok(newStore);
                }
                catch (DbUpdateException)
                {
                    if (storeRepository.GetById(store.Id!.Value) != null)
                        return Conflict();
                }
            }
            return Unauthorized();
        }

        [HttpDelete("{id:guid}")]
        [Authorize]
        public async Task<ActionResult> DeleteStore(Guid? id)
        {
            Store? store = await storeRepository.Delete(id);
            await elasticClient.DeleteAsync(new DeleteRequest(index: "stores", store!.Id));
            return Ok();
        }

        [HttpPut("{id:guid}")]
        [Authorize]
        public async Task<ActionResult> PutStore(Guid id, Store Store)
        {
            try
            {
                if (id != Store.Id) return BadRequest();
                Store? beforeUpdateObj = await storeRepository.GetById(Store.Id.Value);
                if (beforeUpdateObj == null) throw new Exception("Store that need to update does not exist");
                Store updateObj = new Store()
                {
                    Address = Store.Address == null ? beforeUpdateObj.Address : Store.Address,
                    Description = Store.Description == null ? beforeUpdateObj.Description : Store.Description,
                    Id = Store.Id == null ? beforeUpdateObj.Id : Store.Id,
                    PhotoUrl = Store.PhotoUrl == null ? beforeUpdateObj.PhotoUrl : Store.PhotoUrl,
                    Name = Store.Name == null ? beforeUpdateObj.Name : Store.Name,
                    UserId = Store.UserId == null ? beforeUpdateObj.UserId : Store.UserId,
                    Rating = Store.Rating == null ? beforeUpdateObj.Rating : Store.Rating,
                    Status = Store.Status == null ? beforeUpdateObj.Status : Store.Status,
                };
                await storeRepository.Update(updateObj);

                var elasticsearchStore = new ElasticsearchStore
                {
                    Id = updateObj.Id,
                    Name = updateObj.Name,
                    Rating = updateObj.Rating,
                };

                // Does doc exist on Elasticsearch?
                var existsResponse = await elasticClient.DocumentExistsAsync(new DocumentExistsRequest(index: "stores", updateObj.Id));
                if (!existsResponse.Exists)
                {
                    // Doc doesn't exist, create new
                    var createResponse = await elasticClient.CreateAsync<ElasticsearchStore>(elasticsearchStore,
                        x => x
                            .Id(updateObj.Id)
                            .Index("stores")
                        );
                }
                else
                {
                    // Doc exists, update
                    var updateResponse = await elasticClient.UpdateAsync<ElasticsearchStore>(updateObj.Id,
                        x => x
                            .Index("stores")
                            .Doc(elasticsearchStore)
                        );
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (storeRepository.GetById(id) == null)
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        /// <summary>
        /// Returns coupon information if all of the following conditions are met: the coupon existed, the coupon didn't expire, the coupon hadn't reached max uses, the coupon hadn't been used by the user.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("{id:guid}/coupons/code/{code:length(1,24)}")]
        [Authorize]
        public async Task<ActionResult<SimpleCoupon>> GetSimpleCouponOfStoreByCode(Guid id, string code)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                // Check if coupon existed, didn't expire
                var coupon = await couponRepository.GetSimpleCouponOfStoreByCode(id, code);
                if (coupon != null)
                {
                    // Check if coupon had reached max uses
                    if (coupon.UsedCount < coupon.MaxUses)
                    {
                        // Check if coupon had been used by the user
                        var isUsed = await orderRepository.IsCouponInUserOrders((Guid)coupon.Id!, uid);
                        if (!isUsed)
                        {
                            // Coupon weren't used by the user
                            return Ok(coupon);
                        }
                        // Coupon had been used by the user
                        return BadRequest();
                    }
                    return NotFound();
                }
                return NotFound();
            }
            return Unauthorized();
        }

        [HttpGet("{id:guid}/ingredients")]
        [Authorize]
        public async Task<GroceryProductPage> ExploreIngredientsOfStore(Guid id,
           int seed,
           [Range(1, int.MaxValue)] int take = 10,
           [Range(0, int.MaxValue)] int lastKey = 0)
        {
            var products = new GroceryProductPage();
            products.Products = await productRepository.Explore((int)ProductTypeEnum.Ingredient, seed, take, lastKey, id);
            return products;
        }

        [HttpGet("{id:guid}/equipment")]
        [Authorize]
        public async Task<GroceryProductPage> ExploreEquipmentOfStore(Guid id,
           int seed,
           [Range(1, int.MaxValue)] int take = 10,
           [Range(0, int.MaxValue)] int lastKey = 0)
        {
            var products = new GroceryProductPage();
            products.Products = await productRepository.Explore((int)ProductTypeEnum.Tool, seed, take, lastKey, id);
            return products;
        }
    }
}
