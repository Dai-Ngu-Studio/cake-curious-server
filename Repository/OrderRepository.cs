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

namespace Repository
{
    public class OrderRepository : IOrderRepository
    {
        public async Task<IEnumerable<StoreDashboardOrders>> GetOrders(int pageSize, int pageIndex)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<StoreDashboardOrders> orders;
            orders = await db.Orders
                               .Skip((pageIndex - 1) * pageSize)
                               .Take(pageSize).Include(o => o.User).Include(o => o.Store).ProjectToType<StoreDashboardOrders>().ToListAsync();
            return orders;
        }
        public async Task<Order> GetById(Guid id)
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
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
