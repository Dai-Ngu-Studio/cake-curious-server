using BusinessObject;
using CakeCurious_API.Utilities;
using Google.Apis.FirebaseDynamicLinks.v1;
using Google.Apis.FirebaseDynamicLinks.v1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using Repository.Constants.Products;
using Repository.Interfaces;
using Repository.Models.Coupons;
using Repository.Models.Product;
using Repository.Models.Stores;
using System.ComponentModel.DataAnnotations;
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
        private readonly FirebaseDynamicLinksService firebaseDynamicLinksService;

        public StoresController(IStoreRepository _storeRepository, ICouponRepository _couponRepository, IOrderRepository _orderRepository, IProductRepository _productRepository, IElasticClient _elasticClient, FirebaseDynamicLinksService _firebaseDynamicLinksService)
        {
            storeRepository = _storeRepository;
            couponRepository = _couponRepository;
            orderRepository = _orderRepository;
            productRepository = _productRepository;
            elasticClient = _elasticClient;
            firebaseDynamicLinksService = _firebaseDynamicLinksService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<AdminDashboardStorePage>> GetStores(string? search, string? sort, string? filter, [Range(1, int.MaxValue)] int size = 10, [Range(1, int.MaxValue)] int page = 1)
        {
            var result = new AdminDashboardStorePage();
            result.Stores = storeRepository.GetStores(search, sort, filter, size, page);
            result.TotalPage = (int)Math.Ceiling((decimal)storeRepository.CountDashboardStores(search, filter) / size);
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
                    Rating = beforeUpdateObj.Rating,
                    Status = Store.Status == null ? beforeUpdateObj.Status : Store.Status,
                };

                var dynamicLinkResponse = await CreateDynamicLink(updateObj);
                updateObj.ShareUrl = dynamicLinkResponse.ShortLink;

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
            return Ok("Update store successfully.");
        }

        [HttpGet("{id:guid}/coupons")]
        [Authorize]
        public ActionResult<SimpleCouponPage> GetValidSimpleCouponsOfStore(Guid id,
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var couponPage = new SimpleCouponPage();
                couponPage.Coupons = couponRepository.GetValidSimpleCouponsOfStoreForUser(id, uid, (page - 1) * take, take);
                return Ok(couponPage);
            }
            return Forbid();
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
                    if (coupon.MaxUses != null)
                    {
                        if (coupon.UsedCount >= coupon.MaxUses)
                        {
                            return NotFound();
                        }
                    }
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
            return Forbid();
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

        [HttpGet("grocery")]
        [Authorize]
        public async Task<GroceryStorePage> ExploreStores(int seed,
            [Range(1, int.MaxValue)] int take = 10,
            [Range(0, int.MaxValue)] int lastKey = 0)
        {
            var stores = new GroceryStorePage();
            stores.Stores = await storeRepository.Explore(seed, take, lastKey);
            return stores;
        }

        [HttpGet("active-coupons")]
        [Authorize]
        public async Task<ActionResult<ActiveCouponsStorePage>> GetStoresWithActiveCoupons(
            [Range(1, int.MaxValue)] int page = 1,
            [Range(1, int.MaxValue)] int take = 5)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var storePage = new ActiveCouponsStorePage();
                storePage.Stores = await storeRepository.GetActiveCouponsStore(uid, (page - 1) * take, take);
                return Ok(storePage);
            }
            return Forbid();
        }

        private async Task<CreateShortDynamicLinkResponse> CreateDynamicLink(Store store)
        {
            return await DynamicLinkHelper.CreateDynamicLink(
                path: "store-details",
                linkService: firebaseDynamicLinksService,
                id: store.Id.ToString()!,
                name: store.Name!,
                description: store.Description ?? "",
                photoUrl: store.PhotoUrl ?? "",
                thumbnailUrl: null
            );
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<GroceryStorePage>> SearchStores(
            [FromQuery] string? query,
            [FromQuery] string? lastId,
            [FromQuery] double? lastScore,
            [Range(1, int.MaxValue)] int take = 5)
        {
            var searchDescriptor = new SearchDescriptor<ElasticsearchStore>();
            var descriptor = new QueryContainerDescriptor<ElasticsearchStore>();
            var shouldContainer = new List<QueryContainer>();
            var filterContainer = new List<QueryContainer>();

            if (string.IsNullOrWhiteSpace(query))
            {
                var emptyPage = new GroceryStorePage();
                emptyPage.TotalPages = 0;
                emptyPage.Stores = new List<GroceryStore>();

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

            if (lastId != null && lastScore != null)
            {
                searchDescriptor = searchDescriptor.SearchAfter(lastScore, lastId);
            }

            searchDescriptor = searchDescriptor.Index("stores")
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

            var searchResponse = await elasticClient.SearchAsync<ElasticsearchStore>(searchDescriptor);

            var elasticsearchStores = new List<KeyValuePair<Guid, double>>();

            foreach (var hit in searchResponse.Hits)
            {
                elasticsearchStores.Add(
                    new KeyValuePair<Guid, double>((Guid)hit.Source.Id!, (double)hit.Score!)
                );
            }

            var storeIds = elasticsearchStores.Select(x => x.Key).ToList();
            var suggestedStores = await storeRepository.GetSuggestedStores(storeIds);

            var stores = elasticsearchStores
                .Join(suggestedStores, es => es.Key, pd => (Guid)pd.Id!,
                    (es, pd) => { pd.Score = es.Value; return pd; });

            var storePage = new GroceryStorePage();
            storePage.Stores = stores;

            return Ok(storePage);
        }
    }
}
