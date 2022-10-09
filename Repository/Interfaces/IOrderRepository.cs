using BusinessObject;
using Repository.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IOrderRepository
    {
        public IEnumerable<StoreDashboardOrder>? GetOrders(string? s,string? order_by,string? filter_Order,int pageSize, int pageIndex);
        public Task UpdateOrder(Order order);
        public Task<Order?> GetById(Guid guid);
        public int CountDashboardOrders(string? s,string? order_by,string? filter_Order);
        public Task<bool> IsCouponInUserOrders(Guid couponId, string userId);
    }
}
