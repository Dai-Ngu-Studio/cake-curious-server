using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Coupons;
using Repository.Interfaces;
using Repository.Models.Coupons;

namespace Repository
{
    public class CouponRepository : ICouponRepository
    {
        public async Task<SimpleCoupon?> GetSimpleCouponOfStoreByCode(Guid storeId, string code)
        {
            var db = new CakeCuriousDbContext();
            return await db.Coupons
                .AsNoTracking()
                .Where(x => x.StoreId == storeId 
                    && x.Code == code 
                    && x.Status == (int)CouponStatusEnum.Active)
                .ProjectToType<SimpleCoupon>()
                .FirstOrDefaultAsync();
        }
    }
}
