using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public StoresController(IStoreRepository _storeRepository, ICouponRepository _couponRepository, IOrderRepository _orderRepository, IProductRepository _productRepository)
        {
            storeRepository = _storeRepository;
            couponRepository = _couponRepository;
            orderRepository = _orderRepository;
            productRepository = _productRepository;
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

        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<Store>> GetStoresById(Guid guid)
        {
            var result = await storeRepository.GetById(guid);
            return Ok(result);
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public ActionResult<Store> PostStore(Store Store)
        {
            Guid id = Guid.NewGuid();
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Store prod = new Store()
            {
                Address = Store.Address,
                Description = Store.Description,
                Id = id,
                PhotoUrl = Store.PhotoUrl,
                Name = Store.Name,
                UserId = !string.IsNullOrWhiteSpace(uid) ? uid : "",
                Rating = Store.Rating,
                Status = Store.Status,               
            };
            try
            {
                storeRepository.Add(prod);
            }
            catch (DbUpdateException)
            {
                if (storeRepository.GetById(prod.Id!.Value) != null)
                    return Conflict();
            }
            return Ok(prod);
        }

        [HttpDelete("{guid}")]
        [Authorize]
        public async Task<ActionResult> DeleteStore(Guid? guid)
        {
            Store? store = await storeRepository.Delete(guid);
            return Ok("Delete Store " + store!.Name + " success");
        }

        [HttpPut("{guid}")]
        [Authorize]
        public async Task<ActionResult> PutStore(Guid guid, Store Store)
        {
            try
            {
                if (guid != Store.Id) return BadRequest();
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
            }
            catch (DbUpdateConcurrencyException)
            {
                if (storeRepository.GetById(guid) == null)
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
