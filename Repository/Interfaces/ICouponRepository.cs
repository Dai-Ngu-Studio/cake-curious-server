﻿using BusinessObject;
using Repository.Models.Coupons;

namespace Repository.Interfaces
{
    public interface ICouponRepository
    {
        public Task<SimpleCoupon?> GetSimpleCouponOfStoreByCode(Guid storeId, string code);
        public Task<Coupon?> GetById(Guid guid);
        public Task<Coupon?> DeleteCoupon(Guid guid);
        public Task UpdateCoupon(Coupon coupon, string? beforeUpdateCode);
        public Task CreateCoupon(Coupon coupon);
        public IEnumerable<StoreDashboardCoupon>? GetCouponsOfAStore(string uid, string? s, string? order_by, string? filter_Coupon, int pageSize, int pageIndex);
        public int CountCouponPage(string uid, string? s, string? filter_Coupon);
        public Task<SimpleCoupon?> GetActiveSimpleCouponOfStoreById(Guid id, Guid storeId);
        public Task<SimpleCoupon?> GetByIdForWeb(Guid guid);
        public Task UpdateRange(Coupon[] coupons);
        public Task<IEnumerable<Coupon>> GetAllActiveCoupon();
        public Task<ICollection<UserAwareSimpleCoupon>> GetUsableSimpleCouponsOfStoreForUser(Guid storeId, string currentUserId, int skip, int take);
        public Task<ICollection<UserAwareSimpleCoupon>> GetValidSimpleCouponsOfStoreForUser(Guid storeId, string currentUserId, int skip, int take);
    }
}
