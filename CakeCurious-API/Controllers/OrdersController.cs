using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.Orders;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;

        public OrdersController(IOrderRepository _orderRepository)
        {
            orderRepository = _orderRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<StoreDashboardOrders>>> GetProducts(int PageSize, int PageIndex)
        {
            var result = await orderRepository.GetOrders(PageSize, PageIndex);
            return Ok(result);
        }

        [HttpPut("{guid}")]
        public async Task<ActionResult> PutOrder(Guid guid, Order order)
        {

            try
            {
                if (guid != order.Id) return BadRequest();
                Order beforeUpdateOrder = await orderRepository.GetById(order.Id.Value);
                if (beforeUpdateOrder == null) throw new Exception("Order that need to update does not exist");
                Order updateOrder = new Order()
                {
                    Id = beforeUpdateOrder.Id == null ? beforeUpdateOrder.Id : order.Id,
                    CompletedDate = beforeUpdateOrder.CompletedDate == null ? beforeUpdateOrder.CompletedDate : order.CompletedDate,
                    CouponId = beforeUpdateOrder.CouponId == null ? beforeUpdateOrder.CouponId : order.CouponId,
                    Status = beforeUpdateOrder.Status == 0 ? beforeUpdateOrder.Status : order.Status,
                    ProcessedDate = beforeUpdateOrder.ProcessedDate == null ? beforeUpdateOrder.ProcessedDate : order.ProcessedDate,
                    StoreId = beforeUpdateOrder.StoreId == null ? beforeUpdateOrder.StoreId : order.StoreId,
                    OrderDate = beforeUpdateOrder.OrderDate == null ? beforeUpdateOrder.OrderDate : order.OrderDate,
                    Total = beforeUpdateOrder.Total == 0 ? beforeUpdateOrder.Total : order.Total,
                    UserId = beforeUpdateOrder.UserId == null ? beforeUpdateOrder.UserId : order.UserId,

                };
                await orderRepository.UpdateOrder(updateOrder);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (orderRepository.GetById(guid) == null)
                {
                    return NotFound();
                }

                throw;
            }
            return NoContent();
        }
    }
}
