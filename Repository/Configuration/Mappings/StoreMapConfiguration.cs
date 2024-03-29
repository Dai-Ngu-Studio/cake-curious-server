﻿using BusinessObject;
using Mapster;
using Repository.Constants.Coupons;
using Repository.Constants.Products;
using Repository.Models.Orders;
using Repository.Models.Stores;

namespace Repository.Configuration.Mappings
{
    public static class StoreMapConfiguration
    {
        public static void RegisterStoreMapping()
        {
            TypeAdapterConfig<Store, DetailStore>
                .NewConfig()
                .Map(dest => dest.Products, src => src.Products!.Count);

            TypeAdapterConfig<Store, CartOrder>
                .NewConfig()
                .Map(dest => dest.Products, src => src.Products!
                    .Where(x => ((IEnumerable<Guid>)MapContext.Current!.Parameters["productIds"])
                        .Any(y => y == (Guid)x.Id!) && (Guid)x.StoreId! == (Guid)src.Id!))
                .Map(dest => dest.Coupon, src => src.Coupons!
                    .FirstOrDefault(x => ((IEnumerable<Guid?>)MapContext.Current!.Parameters["couponIds"])
                        .Any(y => y == (Guid)x.Id!) && (Guid)x.StoreId! == (Guid)src.Id!))
                .Map(dest => dest.Store, src => src);

            TypeAdapterConfig<Store, BundleOrder>
                .NewConfig()
                .Map(dest => dest.Products, src => src.Products!
                    .Where(x => ((IEnumerable<Guid>)MapContext.Current!.Parameters["productIds"])
                        .Any(y => y == (Guid)x.Id!)
                        && (Guid)x.StoreId! == (Guid)src.Id!
                        && x.Status == (int)ProductStatusEnum.Active))
                .Map(dest => dest.Store, src => src);

            TypeAdapterConfig<Store, ActiveCouponsStore>
                .NewConfig()
                .Map(dest => dest.TotalCoupons, src => src.Coupons!
                    .Where(x => x.Status == (int)CouponStatusEnum.Active)
                    .Where(x => x.ExpiryDate > DateTime.Now)
                    .Where(x => x.MaxUses != null ? x.Orders!.Count < x.MaxUses : true)
                    .Count())
                .Map(dest => dest.Coupons, src => src.Coupons!
                    .Take(5)
                    .OrderByDescending(x => x.Discount)
                    .Where(x => x.Status == (int)CouponStatusEnum.Active)
                    .Where(x => x.ExpiryDate > DateTime.Now)
                    .Where(x => x.MaxUses != null ? x.Orders!.Count < x.MaxUses : true));
        }
    }
}
