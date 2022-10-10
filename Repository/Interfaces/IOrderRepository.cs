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
        public IEnumerable<StoreDashboardOrder>? GetOrdersOfAStore(string uid, string? s, string? order_by, string? filter_Order, int pageSize, int pageIndex);
        public Task UpdateOrder(Order order);
        public Task<Order?> GetById(Guid guid);
        public Task<StoreDashboardOrderDetail?> GetOrderDetailForStore(Guid guid);
        public int CountDashboardOrders(string uid, string? s, string? order_by, string? filter_Order);
    }
}
