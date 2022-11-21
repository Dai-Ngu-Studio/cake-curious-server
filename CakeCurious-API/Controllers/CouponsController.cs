using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.Coupons;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Mapster;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using Repository.Constants.Coupons;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase
    {
        private readonly ICouponRepository _couponRepository;

        public CouponsController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public ActionResult<Coupon> PostCoupon(Coupon coupon)
        {
            Guid id = Guid.NewGuid();
            Coupon obj = new Coupon()
            {
                Id = id,
                Code = coupon.Code,
                Discount = coupon.DiscountType == (int)CouponDiscountTypeEnum.PercentOff ? coupon.Discount >= 1 && coupon.Discount <= 100 ? coupon.Discount / 100 : throw new Exception("Min value is 1 and max value is 100 for PercentOff discount type")
                : coupon.Discount < 1 ? throw new Exception("Min value for FixedDecrease discount type is 1") : coupon.Discount,
                ExpiryDate = coupon.ExpiryDate,
                DiscountType = coupon.DiscountType,
                MaxUses = coupon.MaxUses,
                Name = coupon.Name,
                Status = coupon.Status,
                StoreId = coupon.StoreId,
            };
            try
            {
                _couponRepository.CreateCoupon(obj);
            }
            catch (DbUpdateException )
            {
                if (_couponRepository.GetById(obj.Id.Value) != null)
                    return Conflict();
            }          
            return Ok(obj);
        }
        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<StoreDashboardCouponPage>> GetCoupons(string? search, string? sort, string? filter, [Range(1, int.MaxValue)] int size = 10, [Range(1, int.MaxValue)] int page = 1)
        {
            var result = new StoreDashboardCouponPage();
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            result.Coupons = _couponRepository.GetCouponsOfAStore(uid!, search, sort, filter, size, page);
            result.TotalPage = (int)Math.Ceiling((decimal)_couponRepository.CountCouponPage(uid!, search!, sort!, filter!)! / size);
            return Ok(result);
        }
        [HttpGet("{guid}")]
        [Authorize]
        public ActionResult<SimpleCoupon> GetCouponById(Guid guid)
        {
            return Ok(_couponRepository.GetById(guid).Adapt<SimpleCoupon>());
        }
        [HttpPut("{guid}")]
        [Authorize]
        public async Task<ActionResult> PutCoupon(Guid guid, Coupon coupon)
        {
            try
            {
                if (guid != coupon.Id) return BadRequest();
                Coupon? beforeUpdateObj = await _couponRepository.GetById(coupon.Id.Value);
                if (beforeUpdateObj == null) throw new Exception("Coupon that need to update does not exist.");
                Coupon updateCoupon = new Coupon()
                {
                    Id = coupon.Id == null ? beforeUpdateObj.Id : coupon.Id,
                    Code = coupon.Code == null ? beforeUpdateObj.Code : coupon.Code,
                    Discount = coupon.Discount != null ? coupon.Discount == (int)CouponDiscountTypeEnum.PercentOff ? coupon.Discount / 100 : coupon.Discount : beforeUpdateObj.Discount,
                    ExpiryDate = !coupon.ExpiryDate.HasValue ? beforeUpdateObj.ExpiryDate : coupon.ExpiryDate,
                    DiscountType = coupon.DiscountType == null ? beforeUpdateObj.DiscountType : coupon.DiscountType,
                    MaxUses = coupon.MaxUses == null ? beforeUpdateObj.MaxUses : coupon.MaxUses,
                    Name = coupon.Name == null ? beforeUpdateObj.Name : coupon.Name,
                    Status = coupon.Status == null ? beforeUpdateObj.Status : coupon.Status,
                    StoreId = coupon.StoreId == null ? beforeUpdateObj.StoreId : coupon.StoreId,
                };
                await _couponRepository.UpdateCoupon(updateCoupon);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_couponRepository.GetById(guid) == null)
                {
                    return NotFound();
                }

                throw;
            }
            return NoContent();
        }
        [HttpDelete("{guid}")]
        [Authorize]
        public async Task<ActionResult> HideCoupon(Guid guid)
        {
            Coupon? prod = await _couponRepository.DeleteCoupon(guid);
            return Ok("Delete Coupon " + prod!.Name + " success.");
        }
    }

}
