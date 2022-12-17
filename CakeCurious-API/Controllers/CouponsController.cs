using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.Coupons;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;

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
        public async Task<ActionResult<Coupon>> PostCoupon(Coupon coupon)
        {
            Guid id = Guid.NewGuid();
            Coupon obj = new Coupon()
            {
                Id = id,
                Code = coupon.Code,
                Discount = coupon.Discount,
                ExpiryDate = coupon.ExpiryDate,
                DiscountType = coupon.DiscountType,
                MaxUses = coupon.MaxUses,
                Name = coupon.Name,
                Status = coupon.Status,
                StoreId = coupon.StoreId,
            };
            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            Console.WriteLine("Today:" + today);
            Console.WriteLine("exprire: " + obj.ExpiryDate);
            try
            {
                await _couponRepository.CreateCoupon(obj);
            }
            catch (DbUpdateException)
            {
                if (_couponRepository.GetById(obj.Id.Value) != null)
                    return Conflict();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
            result.TotalPage = (int)Math.Ceiling((decimal)_couponRepository.CountCouponPage(uid!, search!, filter!)! / size);
            return Ok(result);
        }

        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<SimpleCoupon>> GetCouponById(Guid guid)
        {
            return Ok(await _couponRepository.GetByIdForWeb(guid));
        }

        [HttpPut("{guid}")]
        [Authorize]
        public async Task<ActionResult> PutCoupon(Guid guid, Coupon coupon)
        {
            if (guid != coupon.Id) return BadRequest();
            Coupon? beforeUpdateObj = await _couponRepository.GetById(coupon.Id.Value);
            if (beforeUpdateObj == null) throw new Exception("Coupon that need to update does not exist.");
            Coupon updateCoupon = new Coupon()
            {
                Id = coupon.Id == null ? beforeUpdateObj.Id : coupon.Id,
                Code = coupon.Code == null ? beforeUpdateObj.Code : coupon.Code,
                Discount = coupon.Discount == null ? beforeUpdateObj.Discount : coupon.Discount,
                ExpiryDate = !coupon.ExpiryDate.HasValue ? beforeUpdateObj.ExpiryDate : coupon.ExpiryDate,
                DiscountType = coupon.DiscountType == null ? beforeUpdateObj.DiscountType : coupon.DiscountType,
                MaxUses = coupon.MaxUses == null ? beforeUpdateObj.MaxUses : coupon.MaxUses,
                Name = coupon.Name == null ? beforeUpdateObj.Name : coupon.Name,
                Status = coupon.Status == null ? beforeUpdateObj.Status : coupon.Status,
                StoreId = coupon.StoreId == null ? beforeUpdateObj.StoreId : coupon.StoreId,
            };
            try
            {
                await _couponRepository.UpdateCoupon(updateCoupon, beforeUpdateObj.Code);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_couponRepository.GetById(guid) == null)
                {
                    return NotFound();
                }

                throw;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(updateCoupon);
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
