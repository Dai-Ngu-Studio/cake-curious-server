using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Coupons;
using Repository.Constants.Orders;
using Repository.Constants.Products;
using Repository.Constants.Users;
using Repository.Interfaces;
using Repository.Models.OrderDetails;
using Repository.Models.Orders;
using Repository.Models.Stores;
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
            result.TotalPage = (int)Math.Ceiling((decimal)orderRepository.CountDashboardOrders(uid!, search!, filter!) / size);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<StoreDashboardOrder>> GetOrderById(Guid id)
        {
            return Ok(await orderRepository.GetOrderDetailById(id));
        }

        [HttpGet("{id:guid}/details")]
        [Authorize]
        public async Task<ActionResult<DetailOrder>> GetOrderDetails(Guid id)
        {
            var order = await orderRepository.GetOrderDetails(id);
            if (order != null)
            {
                return Ok(order);
            }
            return NotFound();
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
                var errors = new HashSet<CartStore>();
                // Check if there are no orders
                if (checkoutOrders.Orders!.Count() == 0)
                {
                    return BadRequest();
                }
                var orderCount = checkoutOrders.Orders!.Count();
                var successOrderCount = 0;
                // Check each store's order
                foreach (var checkoutOrder in checkoutOrders.Orders!)
                {
                    // Get store in database
                    var store = await storeRepository.GetReadonlyCartStore((Guid)checkoutOrder.StoreId!);
                    if (store != null)
                    {
                        try
                        {
                            // Check if store or store owner is inactive
                            if (store.Status == (int)StoreStatusEnum.Inactive || store.User!.Status == (int)UserStatusEnum.Inactive)
                            {
                                // Store or store owner is inactive
                                throw new Exception();
                            }
                            // Check if order has products
                            if (checkoutOrder.Products!.Count() == 0)
                            {
                                // Order does not contain any products
                                throw new Exception();
                            }

                            // Create query to execute during transaction
                            var queryBuilder = new StringBuilder(checkoutOrder.Products!.Count() * 700);
                            // Declare variables for use in query
                            queryBuilder.AppendLine($"declare @buyQuantity int; declare @newProductQuantityTab table (quantity_value INT NOT NULL); declare @newProductQuantity int;");
                            // Initialize expected rows
                            var expectedRows = 0;
                            // Generate Order ID to reference in query
                            var orderId = Guid.NewGuid();

                            // Create order details and tally total
                            /// Create product quantity update script (for transaction)
                            var orderDetails = new List<OrderDetail>();
                            decimal total = 0.0M;
							var isFirstProduct = true;
                            foreach (var checkoutProduct in checkoutOrder.Products!)
                            {
                                if (checkoutProduct.Quantity == 0)
                                {
                                    // Invalid buy order
                                    throw new Exception();
                                }
                                // Get product from repository
                                /// Get active product only
                                /// Store, store owner must also be active
                                var product = await productRepository.GetActiveProductReadonly((Guid)checkoutProduct.Id!);
                                if (product != null)
                                {
                                    if (checkoutProduct.Quantity! > product.Quantity)
                                    {
                                        // Product quantity in stock does not meet buy order
                                        throw new Exception();
                                    }
                                    if (checkoutProduct.Quantity! <= product.Quantity)
                                    {
                                        // Create order detail
                                        var orderDetail = new OrderDetail
                                        {
                                            ProductId = product.Id,
                                            ProductName = product.Name,
                                            Price = (product.Discount != null) ? product.Price - product.Price * product.Discount : product.Price,
                                            Quantity = checkoutProduct.Quantity!,
                                        };

                                        orderDetails.Add(orderDetail);
                                        expectedRows++; // A row would be created for order detail
                                        total += (decimal)orderDetail.Price! * (decimal)orderDetail.Quantity;
                                        // Update quantity of product
                                        // This query is executed after order (including order details) is created.
                                        /// Query string was not concatenated for performance reason (strings are immutable)
                                        // Product query instructions: 
                                        /*
                                         * 1. Clear previous value in @newProductQuantityTab
                                         * 2. Set @buyQuantity = {orderDetail.Quantity}
                                         * 3. Select product row to lock other transactions from modifying quantity until this transaction is completed
                                         * 4. Update product quantity, set new quantity as @newProductQuantity
                                         * 5. If new product quantity is lower than 0, rollback transaction, an exception will be thrown
                                         */
                                        queryBuilder.AppendLine($"begin delete from @newProductQuantityTab; begin set @buyQuantity = {orderDetail.Quantity} select [p].[quantity] from [Product] as [p] with (updlock) where [p].[id] = '{product.Id}'; update [p] set [p].[quantity] = [p].[quantity] - @buyQuantity output inserted.[quantity] into @newProductQuantityTab from [Product] as [p] with (updlock) where [p].[id] = '{product.Id}' begin set @newProductQuantity = (select [quantity_value] from @newProductQuantityTab) if (@newProductQuantity < 0) begin rollback transaction end end end end");
                                        expectedRows++; // A row of product would be updated (if transaction executed without errors)
										if (isFirstProduct) expectedRows++; // From clearing variable table
										isFirstProduct = false;
                                    }
                                }
                            }

                            var discountedTotal = total; // Initialize discounted total (discounted total = total - discount from coupon)
                            var isCouponApplied = false; // Initialize flag to check if coupon would be applied in this order
                            // Get coupon if existed and apply to total
                            if (checkoutOrder.CouponId != null)
                            {
                                // Get coupon from repository
                                /// Get active coupon only
                                /// Store, store owner must also be active
                                var coupon = await couponRepository.GetActiveSimpleCouponOfStoreById((Guid)checkoutOrder.CouponId!);
                                if (coupon != null)
                                {
                                    // Check if coupon had not reached max uses
                                    /// Only check max uses if coupon is of limited uses type
                                    var isReachedMaxUses = (coupon.MaxUses != null) ? coupon.UsedCount >= coupon.MaxUses : false;
                                    if (!isReachedMaxUses)
                                    {
                                        // Check if coupon had been used by the user
                                        /// Cancelled orders does not count
                                        var isUsed = await orderRepository.IsCouponInUserOrders((Guid)coupon.Id!, uid);
                                        if (!isUsed)
                                        {
                                            // Coupon weren't used by the user
                                            /// Applies the coupon
                                            switch (coupon.DiscountType)
                                            {
                                                case (int)CouponDiscountTypeEnum.PercentOff:
                                                    // Applies percentage coupon
                                                    discountedTotal -= discountedTotal * (decimal)coupon.Discount!;
                                                    isCouponApplied = true;
                                                    break;
                                                case (int)CouponDiscountTypeEnum.FixedDecrease:
                                                    // Check if coupon discount is greater than total of order
                                                    if (discountedTotal >= coupon.Discount)
                                                    {
                                                        // Applies fixed decrease coupon
                                                        discountedTotal -= (decimal)coupon.Discount!;
                                                        isCouponApplied = true;
                                                    }
                                                    break;
                                            }
                                            // If coupon is applied, increase used count
                                            coupon.UsedCount += isCouponApplied ? 1 : 0;
                                        } // End If 
                                        /// Else
                                        // Coupon had been used by the user
                                        /// Do nothing
                                    }

                                    // Check if coupon reached max uses and needed to update status (in transaction)
                                    /// Not neccessary to check if coupon is applied, if coupon got from database had reached max uses, should update its status anyway
                                    if (coupon.MaxUses != null && coupon.UsedCount >= coupon.MaxUses)
                                    {
                                        queryBuilder.AppendLine($"update [Coupon] set [Coupon].[status] = {(int)CouponStatusEnum.Inactive} where [Coupon].[id] = '{coupon.Id}'");
                                        expectedRows++; // A row of coupon would be updated
                                    }
                                }
                            }

                            // Check if orderDetails is empty
                            if (orderDetails.Count > 0)
                            {
                                // Create order
                                var order = new Order
                                {
                                    Id = orderId,
                                    OrderDate = DateTime.Now,
                                    StoreId = checkoutOrder.StoreId,
                                    UserId = uid,
                                    CouponId = isCouponApplied ? checkoutOrder.CouponId : null,
                                    Address = checkoutOrders.Address!.Trim(),
                                    OrderDetails = orderDetails,
                                    Total = total,
                                    DiscountedTotal = discountedTotal <= 0.0M ? 0.0M : discountedTotal,
                                    Status = (int)OrderStatusEnum.Pending,
                                };

                                // Query to delete dangling order (when the order has no order details)
                                // Order query instructions:
                                /// If number of order details with specified orderID equals to 0, the order is dangling
                                //// Delete the dangling order
                                queryBuilder.AppendLine($"if ((select count([od].[id]) from [OrderDetail] as [od] where [od].[order_id] = '{orderId}') = 0) begin delete from [Order] where [Order].[id] = '{orderId}' end");
                                expectedRows++; // A row would be created for order

                                var query = queryBuilder.ToString();

                                /// Transaction should be short to prevent lengthy lock
                                // Add to database, each order should be in its own transaction
                                await orderRepository.AddOrder(order, query, expectedRows);
                                successOrderCount++;
                            }
                            else // No order details, not creating order
                            {
                                // Notify in response order for which store was not created
                                errors.Add(store);
                            }
                        }
                        catch (Exception e) // Exception occurred for an order
                        {
                            // Notify in response the order for which store had an exception
                            Console.WriteLine($"Message: {e.Message}\n{e.InnerException}\n{e.StackTrace}");
                            errors.Add(store);
                            continue; // Continue with other orders
                        }
                    } // End check if store existed
                } // End iteration of orders
                // Check if all order failed
                if (successOrderCount == 0)
                {
                    return Conflict();
                }
                // Some orders were successful
                if (errors.Count > 0)
                {
                    var stores = new CartStores { Stores = errors };
                    return Ok(stores);
                }
                return Ok();
            }
            return Unauthorized();
        }
    }
}
