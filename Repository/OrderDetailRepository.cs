using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        public async Task<OrderDetail?> GetOrderDetail(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.OrderDetails.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task RateOrderDetail(OrderDetail orderDetail)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead))
            {
                await db.Database.ExecuteSqlRawAsync("select [p].[rating] " +
                    "from [Product] as [p] with (updlock) where [p].[id] = {0}", orderDetail.ProductId!);
                db.OrderDetails.Update(orderDetail);
                await db.SaveChangesAsync();
                var count = await db.OrderDetails
                    .Where(x => x.ProductId == orderDetail.ProductId).Where(x => x.Rating != null).CountAsync();
                var sum = await db.OrderDetails
                    .Where(x => x.ProductId == orderDetail.ProductId).Where(x => x.Rating != null).SumAsync(x => x.Rating);
                var rating = sum / count * 1.0M;
                await db.Database.ExecuteSqlRawAsync("update [p] " +
                    "set [p].[rating] = {0} " +
                    "from [Product] as [p] with (updlock) where [p].[id] = {1}", rating, orderDetail.ProductId!);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
        }
    }
}
