using BusinessObject;
using Mapster;
using Repository.Constants.Orders;
using Repository.Models.Coupons;

namespace Repository.Configuration.Mappings
{
    public static class CouponMapConfiguration
    {
        public static void RegisterCouponMapping()
        {
            TypeAdapterConfig<Coupon, SimpleCoupon>
                .NewConfig()
                .Map(dest => dest.UsedCount, src => src.Orders!.Count);

            TypeAdapterConfig<Coupon, UserAwareSimpleCoupon>
                .NewConfig()
                .Map(dest => dest.UsedCount, src => src.Orders!.Count)
                .Map(dest => dest.IsUsedByCurrentUser, src => src.Orders!
                    .Any(y => y.UserId == (string)MapContext.Current!.Parameters["userId"] 
                        && y.Status != (int)OrderStatusEnum.Cancelled));
        }
    }
}
