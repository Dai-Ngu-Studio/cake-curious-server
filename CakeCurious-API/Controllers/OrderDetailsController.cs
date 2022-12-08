using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.OrderDetails;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailsController : ControllerBase
    {
        private readonly IOrderDetailRepository orderDetailRepository;

        public OrderDetailsController(IOrderDetailRepository _orderDetailRepository)
        {
            orderDetailRepository = _orderDetailRepository;
        }

        [HttpPut("{id:guid}/rate")]
        [Authorize]
        public async Task<ActionResult> RateProduct(Guid id, RateOrderDetail rateOrderDetail)
        {
            // check if order detail is of current user, whatever idgaf
            var orderDetail = await orderDetailRepository.GetOrderDetail(id);
            if (orderDetail != null)
            {
                orderDetail.Rating = rateOrderDetail.Rating;
                await orderDetailRepository.RateOrderDetail(orderDetail);
                return Ok();
            }
            return BadRequest();
        }
    }
}
