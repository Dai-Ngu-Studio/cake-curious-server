using BusinessObject;

namespace Repository.Interfaces
{
    public interface IOrderDetailRepository
    {
        public Task<OrderDetail?> GetOrderDetail(Guid id);
        public Task RateOrderDetail(OrderDetail orderDetail);

        public Task<IEnumerable<OrderDetail>> GetOrderDetails(Guid id);
    }
}
