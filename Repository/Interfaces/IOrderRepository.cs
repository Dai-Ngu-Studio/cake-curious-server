using BusinessObject;
using Repository.Models.OrderDetails;
using Repository.Models.Orders;

namespace Repository.Interfaces
{
    public interface IOrderRepository
    {
        public IEnumerable<StoreDashboardOrder>? GetOrdersOfAStore(string uid, string? s, string? order_by, string? filter_Order, int pageSize, int pageIndex);
        public Task UpdateOrder(Order order);
        public Task<StoreDashboardOrder?> GetOrderDetailById(Guid guid);
        public Task<Order?> GetById(Guid guid);
        public Task<IEnumerable<SimpleOrderDetail>?> GetOrderDetailForStore(Guid guid, string? sort, int pageIndex, int pageSize);
        public Task<int?> OrderDetailCount(Guid id, string? sort);
        public int CountDashboardOrders(string uid, string? s, string? order_by, string? filter_Order);
        public Task<bool> IsCouponInUserOrders(Guid couponId, string userId);
        public Task AddOrder(Order order, string query);
        public IEnumerable<InfoOrder> GetOrdersOfUser(string userId, int[] status, int skip, int take);
        public Task<int> CountOrdersOfUser(string userId, int[] status);
        public IEnumerable<string?> GetAddressesOfUser(string userId, int skip, int take);
    }
}
