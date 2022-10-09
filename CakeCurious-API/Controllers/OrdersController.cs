﻿using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Coupons;
using Repository.Constants.Orders;
using Repository.Interfaces;
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
        public ActionResult<IEnumerable<StoreDashboardOrder>> GetOrders(string? search, string? order_by, string? filter, [Range(1, int.MaxValue)] int size = 10, [Range(1, int.MaxValue)] int index = 1)
        {
            var result = new StoreDashboardOrderPage();
            result.Orders = orderRepository.GetOrders(search, order_by, filter, size, index);
            result.TotalPage = (int)Math.Ceiling((decimal)orderRepository.CountDashboardOrders(search!, order_by!, filter!) / size);
            return Ok(result);
        }

        [HttpPut("{guid}")]
        public async Task<ActionResult> PutOrder(Guid guid, Order order)
        {
            try
            {
                if (guid != order.Id) return BadRequest();
                Order? beforeUpdateObj = await orderRepository.GetById(order.Id.Value);
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

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Checkout(CheckoutOrders checkoutOrders)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                // Check each store's order
                foreach (var checkoutOrder in checkoutOrders.Orders!)
                {
                    // Check if store existed
                    if (await storeRepository.IsStoreExisted((Guid)checkoutOrder.StoreId!))
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
                        // to-do: if an order failed, return bad request instead
                    }
                }
                return Ok();
            }
            return Unauthorized();
        }
    }
}
