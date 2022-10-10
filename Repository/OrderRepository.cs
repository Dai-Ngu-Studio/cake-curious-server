using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using Repository.Constants.Orders;

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
                return orders.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).Adapt<IEnumerable<StoreDashboardOrder>>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        public async Task<StoreDashboardOrderDetail?> GetOrderDetailForStore(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Orders.Include(o => o.OrderDetails).Include(o => o.User).Include(o => o.Coupon).ProjectToType<StoreDashboardOrderDetail>().FirstOrDefaultAsync(x => x.Id == id);
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
    }
}
