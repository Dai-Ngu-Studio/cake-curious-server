using BusinessObject;
using Mapster;
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
        }
    }
}
