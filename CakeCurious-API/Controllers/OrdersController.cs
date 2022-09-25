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
                Order beforeUpdateObj= await orderRepository.GetById(order.Id.Value);
                if (beforeUpdateObj == null) throw new Exception("Order that need to update does not exist");
                Order updateOrder = new Order()
                {
                    Id = order.Id == null ? beforeUpdateObj.Id : order.Id,
                    CompletedDate = order.CompletedDate == null ? beforeUpdateObj.CompletedDate : order.CompletedDate,
                    CouponId = order.CouponId == null ? beforeUpdateObj.CouponId : order.CouponId,
                    Status = order.Status == 0 ? beforeUpdateObj.Status : order.Status,
                    ProcessedDate = order.ProcessedDate == null ? beforeUpdateObj.ProcessedDate : order.ProcessedDate,
                    StoreId = order.StoreId == null ? beforeUpdateObj.StoreId : order.StoreId,
                    OrderDate = order.OrderDate == null ? beforeUpdateObj.OrderDate : order.OrderDate,
                    Total = order.Total == 0 ? beforeUpdateObj.Total : order.Total,
                    UserId = order.UserId == null ? beforeUpdateObj.UserId : order.UserId,
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
