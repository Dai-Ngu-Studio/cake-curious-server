using BusinessObject;
using Repository.Models.Orders;

namespace Repository.Interfaces
{
    public interface IOrderRepository
    {
        public IEnumerable<StoreDashboardOrder>? GetOrdersOfAStore(string uid, string? s, string? order_by, string? filter_Order, int pageSize, int pageIndex);
        public Task UpdateOrder(Order order);
        public Task<Order?> GetById(Guid guid);
        public Task<StoreDashboardOrderDetail?> GetOrderDetailForStore(Guid guid);
        public int CountDashboardOrders(string uid, string? s, string? order_by, string? filter_Order);
        public Task<bool> IsCouponInUserOrders(Guid couponId, string userId);
        public Task AddOrder(Order order, string query);
    }
}
