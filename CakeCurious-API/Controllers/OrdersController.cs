using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Coupons;
using Repository.Constants.Orders;
using Repository.Interfaces;
using Repository.Models.OrderDetails;
using Repository.Models.Orders;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        private readonly IStoreRepository storeRepository;
        private readonly IProductRepository productRepository;
        private readonly ICouponRepository couponRepository;

        public OrdersController(IOrderRepository _orderRepository, IStoreRepository _storeRepository, IProductRepository _productRepository, ICouponRepository _couponRepository)
        {
            orderRepository = _orderRepository;
            storeRepository = _storeRepository;
            productRepository = _productRepository;
            couponRepository = _couponRepository;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<StoreDashboardOrder>> GetOrders(string? search, string? sort, string? filter, [Range(1, int.MaxValue)] int size = 10, [Range(1, int.MaxValue)] int page = 1)
        {
            var result = new StoreDashboardOrderPage();
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            result.Orders = orderRepository.GetOrdersOfAStore(uid!, search, sort, filter, size, page);
            result.TotalPage = (int)Math.Ceiling((decimal)orderRepository.CountDashboardOrders(uid!, search!, sort!, filter!) / size);
            return Ok(result);
        }

        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<Order>> GetOrderById(Guid guid)
        {
            return Ok(await orderRepository.GetById(guid));
        }

        [HttpGet("store-order-detail/{guid}")]
        [Authorize]
        public async Task<ActionResult<StoreDashboardOrderDetailPage>> GetStoreOrderDetail(string? sort, string? filter, Guid guid, [Range(1, int.MaxValue)] int page = 1, [Range(1, int.MaxValue)] int size = 10)
        {
            StoreDashboardOrderDetailPage orderDetailPage = new StoreDashboardOrderDetailPage();
            orderDetailPage.orderDetails = await orderRepository.GetOrderDetailForStore(guid, sort, page, size);
            orderDetailPage.TotalPage = (int)Math.Ceiling((decimal)await orderRepository.OrderDetailCount(guid, sort) / size);
            return Ok(orderDetailPage);
        }

        [HttpPut("{guid}")]
        [Authorize]
        public async Task<ActionResult> PutOrder(Guid guid, Order order)
        {
            try
            {
                if (guid != order.Id) return BadRequest();
                Order? beforeUpdateObj = await orderRepository.GetById(order.Id.Value);
                if (beforeUpdateObj == null) throw new Exception("Order that need to update does not exist");
                if (beforeUpdateObj.Status != null
                   && beforeUpdateObj.Status == (int)OrderStatusEnum.Completed)
                {
                    if (order.Status == (int)OrderStatusEnum.Processing
                    || order.Status == (int)OrderStatusEnum.Cancelled
                    || order.Status == (int)OrderStatusEnum.Pending)
                        return BadRequest("Current order status is complete .Can not change to others status");
                }
                else if (beforeUpdateObj.Status != null
                   && beforeUpdateObj.Status == (int)OrderStatusEnum.Cancelled)
                {
                    if (order.Status == (int)OrderStatusEnum.Processing
                    || order.Status == (int)OrderStatusEnum.Completed
                    || order.Status == (int)OrderStatusEnum.Pending)
                        return BadRequest("Current order status is cancelled .Can not change to others status");
                }
                else if (beforeUpdateObj.Status != null
                   && beforeUpdateObj.Status == (int)OrderStatusEnum.Processing)
                {
                    if (order.Status == (int)OrderStatusEnum.Pending)
                        return BadRequest("Current order status is processing .Can not change to others status except completed or cancelled");
                }
                else if (beforeUpdateObj.Status != null
                   && beforeUpdateObj.Status == (int)OrderStatusEnum.Pending)
                {
                    if (order.Status == (int)OrderStatusEnum.Completed)
                        return BadRequest("Current order status is Pending .Can not change to others status except processing or cancelled");
                }
                Order updateOrder = new Order()
                {
                    Id = order.Id == null ? beforeUpdateObj.Id : order.Id,
                    CompletedDate = ((order.Status == (int)OrderStatusEnum.Cancelled) || (order.Status == (int)OrderStatusEnum.Completed && !beforeUpdateObj.CompletedDate.HasValue)) ? DateTime.Now : beforeUpdateObj.CompletedDate,
                    OrderDate = order.OrderDate == null ? beforeUpdateObj.OrderDate : order.OrderDate,
                    ProcessedDate = (order.Status == (int)OrderStatusEnum.Processing && !beforeUpdateObj.ProcessedDate.HasValue) ? DateTime.Now : beforeUpdateObj.ProcessedDate,
                    CouponId = order.CouponId == null ? beforeUpdateObj.CouponId : order.CouponId,
                    Status = order.Status == null ? beforeUpdateObj.Status : order.Status,
                    DiscountedTotal = order.DiscountedTotal == null ? beforeUpdateObj.DiscountedTotal : order.DiscountedTotal,
                    Address = order.Address == null ? beforeUpdateObj.Address : order.Address,
                    StoreId = order.StoreId == null ? beforeUpdateObj.StoreId : order.StoreId,
                    Total = order.Total == null ? beforeUpdateObj.Total : order.Total,
                    UserId = order.UserId ?? beforeUpdateObj.UserId,
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
            return Ok("Update order success");
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Checkout(CheckoutOrders checkoutOrders)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var errors = new HashSet<Guid>();
                // Check each store's order
                foreach (var checkoutOrder in checkoutOrders.Orders!)
                {
                    // Check if store existed
                    if (await storeRepository.IsStoreExisted((Guid)checkoutOrder.StoreId!))
                    {
                        try
                        {
                            // Check if order has products
                            if (checkoutOrder.Products!.Count() == 0)
                            {
                                continue;
                            }

                            // Create query
                            var queryBuilder = new StringBuilder(checkoutOrder.Products!.Count() * 230);

                            // Create order details and tally total
                            /// Create product quantity update script (for transaction)
                            var orderDetails = new List<OrderDetail>();
                            decimal total = 0.0M;
                            foreach (var checkoutProduct in checkoutOrder.Products!)
                            {
                                // Get product from repository
                                var product = await productRepository.GetActiveProductReadonly((Guid)checkoutProduct.Id!);
                                if (product != null)
                                {
                                    var orderDetail = new OrderDetail
                                    {
                                        ProductId = product.Id,
                                        ProductName = product.Name,
                                        Price = (product.Discount != null) ? product.Price * product.Discount : product.Price,
                                        Quantity = (checkoutProduct.Quantity! > product.Quantity) ? product.Quantity : checkoutProduct.Quantity!,
                                    };
                                    orderDetails.Add(orderDetail);
                                    total += (decimal)orderDetail.Price! * (decimal)orderDetail.Quantity;
                                    queryBuilder.AppendLine($"update [Product] set [Product].quantity = [Product].quantity - {orderDetail.Quantity} where [Product].id = '{product.Id}'");
                                }
                            }

                            var discountedTotal = total;
                            // Get coupon if existed and apply to total
                            if (checkoutOrder.CouponId != null)
                            {
                                var coupon = await couponRepository.GetSimpleCouponOfStoreById((Guid)checkoutOrder.CouponId!);
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
                                            /// Applies the coupon
                                            switch (coupon.DiscountType)
                                            {
                                                case (int)CouponDiscountTypeEnum.PercentOff:
                                                    discountedTotal -= discountedTotal * (decimal)coupon.Discount!;
                                                    break;
                                                case (int)CouponDiscountTypeEnum.FixedDecrease:
                                                    discountedTotal -= (decimal)coupon.Discount!;
                                                    break;
                                            }
                                            coupon.UsedCount++;
                                        }
                                        // Coupon had been used by the user
                                        /// Do nothing
                                    }

                                    // Check if coupon reached max uses and needed to update status (in transaction)
                                    if (coupon.UsedCount >= coupon.MaxUses)
                                    {
                                        queryBuilder.AppendLine($"update [Coupon] set [Coupon].status = {(int)CouponStatusEnum.Inactive} where [Coupon].id = '{coupon.Id}'");
                                    }
                                }
                            }

                            // Create order
                            var order = new Order
                            {
                                OrderDate = DateTime.Now,
                                StoreId = checkoutOrder.StoreId,
                                UserId = uid,
                                CouponId = checkoutOrder.CouponId ?? null,
                                Address = checkoutOrders.Address,
                                OrderDetails = orderDetails,
                                Total = total,
                                DiscountedTotal = discountedTotal <= 0.0M ? 0.0M : discountedTotal,
                                Status = (int)OrderStatusEnum.Pending,
                            };

                            var query = queryBuilder.ToString();

                            // Check if orderDetails is empty
                            if (orderDetails.Count > 0)
                            {
                                // Add to database, each order should be in its own transaction
                                await orderRepository.AddOrder(order, query);
                            }
                            else
                            {
                                // Notify
                                errors.Add((Guid)checkoutOrder.StoreId!);
                            }
                        }
                        catch (Exception)
                        {
                            errors.Add((Guid)checkoutOrder.StoreId!);
                            continue;
                        }
                    }
                }
                return Ok(errors.Count > 0 ? errors : string.Empty);
            }
            return Unauthorized();
        }
    }
}
