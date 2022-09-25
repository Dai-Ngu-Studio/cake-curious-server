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
                    Id = order.Id == null ? beforeUpdateOrder.Id : order.Id,
                    CompletedDate = order.CompletedDate == null ? beforeUpdateOrder.CompletedDate : order.CompletedDate,
                    CouponId = order.CouponId == null ? beforeUpdateOrder.CouponId : order.CouponId,
                    Status = order.Status == 0 ? beforeUpdateOrder.Status : order.Status,
                    ProcessedDate = order.ProcessedDate == null ? beforeUpdateOrder.ProcessedDate : order.ProcessedDate,
                    StoreId = order.StoreId == null ? beforeUpdateOrder.StoreId : order.StoreId,
                    OrderDate = order.OrderDate == null ? beforeUpdateOrder.OrderDate : order.OrderDate,
                    Total = order.Total == 0 ? beforeUpdateOrder.Total : order.Total,
                    UserId = order.UserId == null ? beforeUpdateOrder.UserId : order.UserId,
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
