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
            return orders.OrderBy(p => p.OrderDate).ToList();
        }
        public IEnumerable<Order> OrderDescOrderDate(IEnumerable<Order> orders)
        {
            return orders.OrderByDescending(p => p.OrderDate).ToList();
        }
        public IEnumerable<Order> FilterByStatusComplete(IEnumerable<Order> orders)
        {
            return orders.Where(p => p.Status == (int)OrderStatusEnum.Completed).ToList();
        }
        public IEnumerable<Order> FilterByStatusProcessing(IEnumerable<Order> orders)
        {
            return orders.Where(p => p.Status == (int)OrderStatusEnum.Processing).ToList();
        }
        public IEnumerable<Order> FilterByStatusCancelled(IEnumerable<Order> orders)
        {
            return orders.Where(p => p.Status == (int)OrderStatusEnum.Cancelled).ToList();
        }
        public IEnumerable<Order> FilterByStatusPending(IEnumerable<Order> orders)
        {
            return orders.Where(p => p.Status == (int)OrderStatusEnum.Pending).ToList();
        }

        public IEnumerable<Order> SearchOrder(string? keyWord, IEnumerable<Order> orders)
        {
            return orders.Where(p => p.User!.DisplayName!.Contains(keyWord!)).ToList();
        }

        public IEnumerable<StoreDashboardOrder>? GetOrdersOfAStore(string uid, string? s, string? order_by, string? filter_Order, int pageSize, int pageIndex)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Order> orders = db.Orders.Include(o => o.OrderDetails).Include(o => o.User).Include(o => o.Store).Where(o => o.Store!.UserId == uid).ToList();
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
        public IEnumerable<SimpleOrderDetail> OrderByAscPrice(IEnumerable<SimpleOrderDetail> orderdetail)
        {
            return orderdetail.OrderBy(p => p.Price).ToList();
        }

        public IEnumerable<SimpleOrderDetail> OrderByDescPrice(IEnumerable<SimpleOrderDetail> orderdetail)
        {

            return orderdetail.OrderByDescending(p => p.Price).ToList();
        }
        public async Task<StoreDashboardOrderDetail?> GetOrderDetailForStore(Guid id, string? sort, int pageIndex, int pageSize)
        {
            try
            {
                var db = new CakeCuriousDbContext();
                StoreDashboardOrderDetail? orderDetail = await db.Orders.Include(o => o.OrderDetails).Include(o => o.User).Include(o => o.Coupon).ProjectToType<StoreDashboardOrderDetail>().FirstOrDefaultAsync(x => x.Id == id);
                if (sort != null && sort == OrderByEnum.ByAscPrice.ToString())
                {
                    orderDetail!.OrderDetails = OrderByAscPrice(orderDetail!.OrderDetails!);
                }
                else if (sort != null && sort == OrderByEnum.ByDescPrice.ToString())
                {
                    orderDetail!.OrderDetails = OrderByDescPrice(orderDetail!.OrderDetails!);
                }
                orderDetail!.OrderDetails = orderDetail!.OrderDetails!.Skip((pageIndex! - 1) * pageSize)
                            .Take(pageSize).ToList();
                return orderDetail;
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
                StoreDashboardOrderDetail? orderDetail = await db.Orders.Include(o => o.OrderDetails).Include(o => o.User).Include(o => o.Coupon).ProjectToType<StoreDashboardOrderDetail>().FirstOrDefaultAsync(x => x.Id == id);
                if (sort != null && sort == OrderByEnum.ByAscPrice.ToString())
                {
                    orderDetail!.OrderDetails = OrderByAscPrice(orderDetail!.OrderDetails!);
                }
                else if (sort != null && sort == OrderByEnum.ByDescPrice.ToString())
                {
                    orderDetail!.OrderDetails = OrderByDescPrice(orderDetail!.OrderDetails!);
                }
                return orderDetail!.OrderDetails!.Count();
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
                db.Entry<Order>(updateObj).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public int CountDashboardOrders(string uid, string? s, string? order_by, string? filter_Order)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Order> orders = db.Orders.Include(o => o.User).Include(o => o.Store).Where(o => o.Store!.UserId == uid).ToList();
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
                //sort
                if (order_by != null && order_by == OrderSortEnum.DescOrderDate.ToString())
                {
                    orders = OrderDescOrderDate(orders);
                }
                else if (order_by != null && order_by == OrderSortEnum.AscOrderDate.ToString())
                {
                    orders = OrderAscOrderDate(orders);
                }
                return orders.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }

        public async Task<Order?> GetById(Guid guid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Orders.FirstOrDefaultAsync(x => x.Id == guid);
        }

        public async Task AddOrder(Order order, string query)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                await db.Orders.AddAsync(order);
                if (!string.IsNullOrWhiteSpace(query))
                {
                    await db.Database.ExecuteSqlRawAsync(query);
                }
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Find if coupon were used by user in pending or processing or finished order.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> IsCouponInUserOrders(Guid couponId, string userId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Orders.AnyAsync(x => x.UserId == userId
                && x.CouponId == couponId
                && (x.Status == (int)OrderStatusEnum.Pending
                    || x.Status == (int)OrderStatusEnum.Processing
                    || x.Status == (int)OrderStatusEnum.Completed));
        }
    }
}
