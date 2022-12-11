using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.DeactivateReasons;

namespace Repository
{
    public class DeactivateReasonRepository : IDeactivateReasonRepository
    {
        public async Task CreateReason(DeactivateReason reason)
        {
            var db = new CakeCuriousDbContext();
            await db.DeactivateReasons.AddAsync(reason);
            await db.SaveChangesAsync();
        }

        public async Task<DetailDeactivateReason?> GetReasonByItemIdReadonly(Guid itemId)
        {
            var db = new CakeCuriousDbContext();
            return await db.DeactivateReasons
                .AsNoTracking()
                .OrderByDescending(x => x.DeactivateDate)
                .Where(x => x.ItemId == itemId)
                .ProjectToType<DetailDeactivateReason>()
                .FirstOrDefaultAsync();
        }
    }
}
