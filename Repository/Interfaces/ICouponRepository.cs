using Repository.Models.Coupons;

namespace Repository.Interfaces
{
    public interface ICouponRepository
    {
        public Task<SimpleCoupon?> GetSimpleCouponOfStoreByCode(Guid storeId, string code);
    }
}
