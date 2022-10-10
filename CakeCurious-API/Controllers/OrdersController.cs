using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.Orders;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
        public ActionResult<IEnumerable<StoreDashboardOrder>> GetOrders(string? search, string? sort, string? filter, [Range(1, int.MaxValue)] int size = 10, [Range(1, int.MaxValue)] int index = 1)
        {
            var result = new StoreDashboardOrderPage();
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            result.Orders = _orderRepository.GetOrdersOfAStore(uid!, search, sort, filter, size, index);
            result.TotalPage = (int)Math.Ceiling((decimal)_orderRepository.CountDashboardOrders(uid!, search!, sort!, filter!) / size);
            return Ok(result);
        }

        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<Order>> GetOrderById(Guid guid)
        {
            return Ok(await _orderRepository.GetById(guid));
        }

        [HttpGet("store-order-detail/{guid}")]
        [Authorize]
        public async Task<ActionResult<StoreDashboardOrderDetail>> GetStoreOrderDetail(Guid guid)
        {
            return Ok(await _orderRepository.GetOrderDetailForStore(guid));
        }

        [HttpPut("{guid}")]
        [Authorize]
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
