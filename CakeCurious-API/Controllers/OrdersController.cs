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
        private readonly IOrderRepository _orderRepository;

        public OrdersController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<StoreDashboardOrder>> GetOrders(string? s, string? filter_Order, int PageSize, int PageIndex)
        {
            var result = new StoreDashboardOrderPage();
            result.Orders = _orderRepository.GetOrders(s, filter_Order, PageSize, PageIndex);
            result.TotalPage = (int)Math.Ceiling((decimal)_orderRepository.CountDashboardOrders(s!, filter_Order!) / PageSize);
            return Ok(result);
        }

        [HttpPut("{guid}")]
        public async Task<ActionResult> PutOrder(Guid guid, Order order)
        {
            try
            {
                if (guid != order.Id) return BadRequest();
                Order? beforeUpdateObj = await _orderRepository.GetById(order.Id.Value);
                if (beforeUpdateObj == null) throw new Exception("Order that need to update does not exist");
                Order updateOrder = new Order()
                {
                    Id = order.Id == null ? beforeUpdateObj.Id : order.Id,
                    CompletedDate = order.CompletedDate == null ? beforeUpdateObj.CompletedDate : order.CompletedDate,
                    CouponId = order.CouponId == null ? beforeUpdateObj.CouponId : order.CouponId,
                    Status = order.Status == null ? beforeUpdateObj.Status : order.Status,
                    ProcessedDate = order.ProcessedDate == null ? beforeUpdateObj.ProcessedDate : order.ProcessedDate,
                    StoreId = order.StoreId == null ? beforeUpdateObj.StoreId : order.StoreId,
                    OrderDate = order.OrderDate == null ? beforeUpdateObj.OrderDate : order.OrderDate,
                    Total = order.Total == null ? beforeUpdateObj.Total : order.Total,
                    UserId = order.UserId ?? beforeUpdateObj.UserId,
                };
                await _orderRepository.UpdateOrder(updateOrder);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_orderRepository.GetById(guid) == null)
                {
                    return NotFound();
                }

                throw;
            }
            return NoContent();
        }
    }
}
