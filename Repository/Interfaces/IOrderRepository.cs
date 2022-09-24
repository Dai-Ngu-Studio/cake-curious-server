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
        public Task<IEnumerable<StoreDashboardOrders>> GetOrders(int pageSize, int pageIndex);
        public Task UpdateOrder(Order order);
        public Task<Order> GetById(Guid guid);
    }
}
