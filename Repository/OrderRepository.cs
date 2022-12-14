using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.Orders;
using Mapster;
using Repository.Constants.Orders;
using Repository.Models.OrderDetails;
using Repository.Constants.OrderDetails;

namespace Repository
{
    public class OrderRepository : IOrderRepository
    {
        public IEnumerable<Order> OrderAscOrderDate(IEnumerable<Order> orders)
        {
            return orders.OrderBy(p => p.OrderDate);
        }

        public IEnumerable<Order> OrderDescOrderDate(IEnumerable<Order> orders)
        {
            return orders.OrderByDescending(p => p.OrderDate);
        }

        public IEnumerable<Order> FilterByStatusComplete(IEnumerable<Order> orders)
        {
            return orders.Where(p => p.Status == (int)OrderStatusEnum.Completed);
        }

        public IEnumerable<Order> FilterByStatusProcessing(IEnumerable<Order> orders)
        {
            return orders.Where(p => p.Status == (int)OrderStatusEnum.Processing);
        }

        public IEnumerable<Order> FilterByStatusCancelled(IEnumerable<Order> orders)
        {
            return orders.Where(p => p.Status == (int)OrderStatusEnum.Cancelled);
        }

        public IEnumerable<Order> FilterByStatusPending(IEnumerable<Order> orders)
        {
            return orders.Where(p => p.Status == (int)OrderStatusEnum.Pending);
        }

        public IEnumerable<Order> SearchOrder(string? keyWord, IEnumerable<Order> orders)
        {
            return orders.Where(p => p!.Id!.Value.ToString().Contains(keyWord!));
        }

        public IEnumerable<StoreDashboardOrder>? GetOrdersOfAStore(string uid, string? s, string? order_by, string? filter_Order, int pageSize, int pageIndex)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Order> orders = db.Orders.Include(o => o.User).Include(o => o.Store).Where(o => o.Store!.UserId == uid).OrderByDescending(o => o.OrderDate);
            try
            {
                if (!string.IsNullOrWhiteSpace(s))
                {
                    orders = SearchOrder(s, orders);
                }
                //filter
                if (filter_Order != null && filter_Order == OrderStatusEnum.Pending.ToString())
                {
                    orders = FilterByStatusPending(orders);
                }
                else if (filter_Order != null && filter_Order == OrderStatusEnum.Completed.ToString())
                {
                    orders = FilterByStatusComplete(orders);
                }
                else if (filter_Order != null && filter_Order == OrderStatusEnum.Processing.ToString())
                {
                    orders = FilterByStatusProcessing(orders);
                }
                else if (filter_Order != null && filter_Order == OrderStatusEnum.Cancelled.ToString())
                {
                    orders = FilterByStatusCancelled(orders);
                }
                //sort
                if (order_by != null && order_by == OrderSortEnum.DescOrderDate.ToString())
                {
                    orders = OrderDescOrderDate(orders);
                }
                else if (order_by != null && order_by == OrderSortEnum.AscOrderDate.ToString())
                {
                    orders = OrderAscOrderDate(orders);
                }
                return orders.Adapt<IEnumerable<StoreDashboardOrder>>().Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        public IEnumerable<OrderDetail> OrderByAscPrice(IEnumerable<OrderDetail> orderdetail)
        {
            return orderdetail.OrderBy(p => p.Price).ToList();
        }

        public IEnumerable<OrderDetail> OrderByDescPrice(IEnumerable<OrderDetail> orderdetail)
        {

            return orderdetail.OrderByDescending(p => p.Price).ToList();
        }
        public async Task<IEnumerable<SimpleOrderDetail>?> GetOrderDetailForStore(Guid id, string? sort, int pageIndex, int pageSize)
        {
            try
            {
                var db = new CakeCuriousDbContext();
                IEnumerable<OrderDetail>? orderDetails = await db.OrderDetails.Where(x => x!.Order!.Id == id).ToListAsync();
                if (sort != null && sort == OrderByEnum.ByAscPrice.ToString())
                {
                    orderDetails = OrderByAscPrice(orderDetails);
                }
                else if (sort != null && sort == OrderByEnum.ByDescPrice.ToString())
                {
                    orderDetails = OrderByDescPrice(orderDetails);
                }
                return orderDetails.Skip((pageIndex! - 1) * pageSize)
                            .Take(pageSize).ToList().Adapt<IEnumerable<SimpleOrderDetail>>();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<int?> OrderDetailCount(Guid id, string? sort)
        {
            try
            {
                var db = new CakeCuriousDbContext();
                IEnumerable<OrderDetail>? orderDetails = await db.OrderDetails.Where(x => x!.Order!.Id == id).ToListAsync();
                if (sort != null && sort == OrderByEnum.ByAscPrice.ToString())
                {
                    orderDetails = OrderByAscPrice(orderDetails);
                }
                else if (sort != null && sort == OrderByEnum.ByDescPrice.ToString())
                {
                    orderDetails = OrderByDescPrice(orderDetails);
                }
                return orderDetails.Count();
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateOrder(Order updateObj)
        {
            try
            {
                var db = new CakeCuriousDbContext();
                //Add Product Back To DB
                if (updateObj.Status == (int)OrderStatusEnum.Cancelled)
                {
                    IEnumerable<OrderDetail> orderDetails = await db.OrderDetails.Include(od => od.Product).Where(od => od.OrderId == updateObj.Id).ToListAsync();
                    foreach (var orderDetail in orderDetails)
                    {
                        orderDetail!.Product!.Quantity = orderDetail!.Product!.Quantity + orderDetail.Quantity;
                        db.Entry<Product>(orderDetail.Product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                }
                db.Entry<Order>(updateObj).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public int CountDashboardOrders(string uid, string? s, string? filter_Order)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Order> orders = db.Orders.Include(o => o.User).Include(o => o.Store).Where(o => o.Store!.UserId == uid);
            try
            {
                if (s != null)
                {
                    orders = SearchOrder(s, orders);
                }
                //filter
                if (filter_Order != null && filter_Order == OrderStatusEnum.Pending.ToString())
                {
                    orders = FilterByStatusPending(orders);
                }
                else if (filter_Order != null && filter_Order == OrderStatusEnum.Completed.ToString())
                {
                    orders = FilterByStatusComplete(orders);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return orders.Count();
        }

        public async Task<StoreDashboardOrder?> GetOrderDetailById(Guid guid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Orders.ProjectToType<StoreDashboardOrder>().FirstOrDefaultAsync(x => x.Id == guid);
        }

        public async Task<Order?> GetById(Guid guid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Orders.Include(o => o.Coupon).FirstOrDefaultAsync(x => x.Id == guid);
        }

        public async Task AddOrder(Order order, string query)
        {
            var db = new CakeCuriousDbContext();
            // Starts transaction, update to products are locked for others
            /// Which is why transaction should be short
            using (var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead))
            {
                await db.Orders.AddAsync(order); // Add order (including order details)
                await db.SaveChangesAsync();
                if (!string.IsNullOrWhiteSpace(query))
                {
                    // Execute query to update product quantity
                    await db.Database.ExecuteSqlRawAsync(query);
                }
                await db.SaveChangesAsync();
                await transaction.CommitAsync(); // Commit transaction, remove lock
            }
        }

        public async Task<int> CountOrdersOfUser(string userId, int[] status)
        {
            var db = new CakeCuriousDbContext();
            return await db.Orders
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Where(x => status.Any(y => y == (int)x.Status!))
                .CountAsync();
        }

        public IEnumerable<InfoOrder> GetOrdersOfUser(string userId, int[] status, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Orders
                .AsNoTracking()
                .OrderByDescending(x => x.OrderDate)
                .Where(x => x.UserId == userId)
                .Where(x => status.Any(y => y == (int)x.Status!))
                .Skip(skip)
                .Take(take)
                .ProjectToType<InfoOrder>();
        }

        public IEnumerable<string?> GetAddressesOfUser(string userId, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Orders
                .AsNoTracking()
                .OrderByDescending(x => x.OrderDate)
                .Where(x => x.UserId == userId)
                .Select(x => x.Address)
                .Distinct()
                .Skip(skip)
                .Take(take);
        }

        public async Task<DetailOrder?> GetOrderDetails(Guid id)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("productIngredients", new Dictionary<Guid, string>());

                var db = new CakeCuriousDbContext();
                return await db.Orders
                    .AsNoTracking()
                    .ProjectToType<DetailOrder>()
                    .FirstOrDefaultAsync(x => (Guid)x.Id! == id);
            }
        }

        /// <summary>
        /// Find if coupon were used by user in pending or processing or finished order.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsCouponInUserOrders(Guid couponId, string userId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Orders
                .AsNoTracking()
                .AsSplitQuery()
                .AnyAsync(x => x.UserId == userId
                && x.CouponId == couponId
                && (x.Status == (int)OrderStatusEnum.Pending
                    || x.Status == (int)OrderStatusEnum.Processing
                    || x.Status == (int)OrderStatusEnum.Completed));
        }
    }
}
