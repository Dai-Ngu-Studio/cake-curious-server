using BusinessObject;
using CakeCurious_API.Utilities;
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
                var errors = new HashSet<Guid>();
                // Check if there are no orders
                if (checkoutOrders.Orders!.Count() == 0)
                {
                    return BadRequest();
                }
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
                                // Order does not contain any products
                                // Continue with other orders
                                continue;
                            }

                            // Create query to execute during transaction
                            var queryBuilder = new StringBuilder(checkoutOrder.Products!.Count() * 800);
                            // Declare variables for use in query
                            queryBuilder.AppendLine($"declare @initialProductQuantity int; declare @buyQuantity int; declare @newProductQuantityTab table (quantity_value INT NOT NULL); declare @orderDetailNewQuantityTab table (quantity_value INT NOT NULL); declare @newProductQuantity int; declare @orderDetailNewQuantity int;");
                            // Initialize expected rows
                            var expectedRows = 0;
                            // Generate Order ID to reference in query
                            var orderId = Guid.NewGuid();

                            // Create order details and tally total
                            /// Create product quantity update script (for transaction)
                            var orderDetails = new List<OrderDetail>();
                            decimal total = 0.0M;
                            foreach (var checkoutProduct in checkoutOrder.Products!)
                            {
                                // Get product from repository
                                /// Get active product only
                                /// Store, store owner must also be active
                                var product = await productRepository.GetActiveProductReadonly((Guid)checkoutProduct.Id!);
                                if (product != null)
                                {
                                    var actualBuyQuantity = (checkoutProduct.Quantity! > product.Quantity) ? product.Quantity : checkoutProduct.Quantity!;
                                    // Actual buy quantity must be greater than 0
                                    if (actualBuyQuantity > 0)
                                    {
                                        // Create order detail
                                        var orderDetail = new OrderDetail
                                        {
                                            ProductId = product.Id,
                                            ProductName = product.Name,
                                            Price = (product.Discount != null) ? product.Price - product.Price * product.Discount : product.Price,
                                            Quantity = actualBuyQuantity,
                                        };

                                        orderDetails.Add(orderDetail);
                                        expectedRows++; // A row would be created for order detail
                                        total += (decimal)orderDetail.Price! * (decimal)orderDetail.Quantity;
                                        // Update quantity of product
                                        // This query is executed after order (including order details) is created.
                                        /// Query string was not concatenated for performance reason (strings are immutable)
                                        // Product query instructions: 
                                        /*
                                         * 1. Set value for variable @productQuantity (stock quantity of product: product.quantity), also use update lock on the selected product to prevent modification during transaction
                                         * 2. Set value for variable @productNewQuantity (quantity to update product to: @productQuantity - orderDetail.quantity)
                                         * 3. Update quantity of product to @productNewQuantity if @productNewQuantity >= 0, else update quantity of product to 0 (the version must match for the update to affect)
                                         * 4. Check the numbers of rows affected for the update statement
                                         * 4.A. If no rows are affected, the product quantity had already been changed by another transaction (version of product changed during transaction)
                                         * 4.A.1 Delete the order detail for product
                                         * 4.B. If rows are affected, the product quantity was not changed by another transaction (version of product did not change during transaction)
                                         * 4.B.A. If @productNewQuantity is lower than 0, there are not enough products to meet order requirement
                                         * 4.B.A.A. If @productQuantity is lower or equals to 0, product is already out of stock
                                         * 4.B.A.A.1. Delete the order detail for product (if this order detail is the only order detail in order, the order will be deleted later in the query below)
                                         * 4.B.A.B. Else, stock quantity does not fully meet order requirement but is not 0
                                         * 4.B.A.B.1. Update the quantity of order detail to equal stock quantity
                                         */
                                        queryBuilder.AppendLine($"begin delete from @newProductQuantityTab; delete from @orderDetailNewQuantityTab; begin set @buyQuantity = {orderDetail.Quantity} set @initialProductQuantity = (select [p].[quantity] from [Product] as [p] with (updlock) where [p].[id] = '{product.Id}'); update [p] set [p].[quantity] = [p].[quantity] - @buyQuantity output inserted.[quantity] into @newProductQuantityTab from [Product] as [p] with (updlock) where [p].[id] = '{product.Id}' begin set @newProductQuantity = (select [quantity_value] from @newProductQuantityTab) if (@newProductQuantity < 0) begin if (@initialProductQuantity <= 0) begin delete from [OrderDetail] where [OrderDetail].[order_id] = '{orderId}' and [OrderDetail].[product_id] = '{product.Id}'  end else begin update [od] set [od].[quantity] = @newProductQuantity + @buyQuantity output inserted.[quantity] into @orderDetailNewQuantityTab from [OrderDetail] as [od] where [od].[order_id] = '{orderId}' and [od].[product_id] = '{product.Id}'; set @orderDetailNewQuantity = (select [quantity_value] from @orderDetailNewQuantityTab) if (@orderDetailNewQuantity <= 0) begin delete from [OrderDetail] where [OrderDetail].[order_id] = '{orderId}' and [OrderDetail].[product_id] = '{product.Id}' end end begin update [p] set [p].[quantity] = 0 from [Product] as [p] where [p].[id] = '{product.Id}' end end end end end");
                                        expectedRows++; // A row of product would be updated (if no invalid order or order details are detected)
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
                            }
                            else // No order details, not creating order
                            {
                                // Notify in response order for which store was not created
                                errors.Add((Guid)checkoutOrder.StoreId!);
                            }
                        }
                        catch (Exception e) // Exception occurred for an order
                        {
                            // Notify in response the order for which store had an exception
                            Console.WriteLine($"Message: {e.Message}\n{e.InnerException}\n{e.StackTrace}");
                            errors.Add((Guid)checkoutOrder.StoreId!);
                            continue; // Continue with other orders
                        }
                    } // End check if store existed
                } // End iteration of orders
                return Ok(errors.Count > 0 ? errors : string.Empty);
            }
            return Unauthorized();
        }
    }
}
