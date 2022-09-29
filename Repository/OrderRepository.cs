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
        public IEnumerable<StoreDashboardOrder> FilterByAscOrderDate(IEnumerable<StoreDashboardOrder> orders)
        {
            return orders.OrderBy(p => p.OrderDate).ToList();
        }
        public IEnumerable<StoreDashboardOrder> FilterByDescOrderDate(IEnumerable<StoreDashboardOrder> orders)
        {
            return orders.OrderByDescending(p => p.OrderDate).ToList();
        }
        public IEnumerable<StoreDashboardOrder> FilterByStatusComplelted(IEnumerable<StoreDashboardOrder> orders)
        {
            return orders.Where(p => p.Status == (int)OrderStatusEnum.Completed).ToList();
        }

        public IEnumerable<StoreDashboardOrder> FilterByStatusPending(IEnumerable<StoreDashboardOrder> orders)
        {
            return orders.Where(p => p.Status == (int)OrderStatusEnum.Pending).ToList();
        }

        public IEnumerable<StoreDashboardOrder> SearchOrder(string? keyWord)
        {
            IEnumerable<StoreDashboardOrder>? orders;
            var db = new CakeCuriousDbContext();
            orders = keyWord != null
                    ? db.Orders.Include(o => o.User).Where(p => p.User!.DisplayName!.Contains(keyWord!)).ProjectToType<StoreDashboardOrder>().ToList()
                    : db.Orders.Include(o => o.User).ProjectToType<StoreDashboardOrder>().ToList();
            return orders;
        }
        public IEnumerable<StoreDashboardOrder>? GetOrders(string? s, string? filter_Order, int pageSize, int pageIndex)
        {
            IEnumerable<StoreDashboardOrder> result;
            IEnumerable<StoreDashboardOrder> orders = SearchOrder(s);
            try
            {
                if (filter_Order == null)
                    return orders.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ToList();
                else if (filter_Order == "ByStatusPending")
                {
                    result = FilterByStatusPending(orders);
                    Console.WriteLine("Ket qua filter co " + result.Count());

                    return result.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ToList();
                }
                else if (filter_Order == "ByStatusCompleted")
                {
                    result = FilterByStatusComplelted(orders);
                    return result.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ToList();
                }
                else if (filter_Order == "ByDescendingOrderDate")
                {
                    result = FilterByDescOrderDate(orders);
                    return result.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ToList();
                }
                else if (filter_Order == "ByAcsendingOrderDate")
                {
                    result = FilterByAscOrderDate(orders);
                    return result.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        public async Task<Order?> GetById(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Orders.FirstOrDefaultAsync(x => x.Id == id);
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

        public int CountDashboardOrders(string? s, string? filter_Order)
        {
            IEnumerable<StoreDashboardOrder> result;
            IEnumerable<StoreDashboardOrder> orders = SearchOrder(s);
            try
            {
                if (filter_Order == null)
                    return orders.Count();
                else if (filter_Order == "ByStatusPending")
                {
                    result = FilterByStatusPending(orders);
                    Console.WriteLine("Ket qua filter co " + result.Count());

                    return result.Count();
                }
                else if (filter_Order == "ByStatusCompleted")
                {
                    result = FilterByStatusComplelted(orders);
                    return result.Count();
                }
                else if (filter_Order == "ByDescendingOrderDate")
                {
                    result = FilterByDescOrderDate(orders);
                    return result.Count();
                }
                else if (filter_Order == "ByAcsendingOrderDate")
                {
                    result = FilterByAscOrderDate(orders);
                    return result.Count();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }
    }
}
