using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Coupons;
using Repository.Constants.Products;
using Repository.Constants.Users;
using Repository.Interfaces;
using Repository.Models.Coupons;

namespace Repository
{
    public class CouponRepository : ICouponRepository
    {
        public IEnumerable<Coupon> SearchCoupon(string? keyWord, IEnumerable<Coupon> coupons)
        {
            coupons = coupons.Where(p => p!.Name!.ToLower().Contains(keyWord!.ToLower()));
            return coupons;
        }
        public IEnumerable<Coupon> OrderByAscExpireDate(IEnumerable<Coupon> coupons)
        {

            return coupons.OrderBy(p => p.ExpiryDate);
        }
        public IEnumerable<Coupon> OrderByDescExpireDate(IEnumerable<Coupon> coupons)
        {
            return coupons.OrderByDescending(p => p.ExpiryDate);
        }
        public IEnumerable<Coupon> FilterByActiveStatus(IEnumerable<Coupon> coupons)
        {
            return coupons.Where(p => p.Status == (int)CouponStatusEnum.Active);
        }
        public IEnumerable<Coupon> FilterByInActiveStatus(IEnumerable<Coupon> coupons)
        {
            return coupons.Where(p => p.Status == (int)CouponStatusEnum.Active);
        }
        public IEnumerable<StoreDashboardCoupon>? GetCouponsOfAStore(string uid, string? s, string? order_by, string? filter_Coupon, int pageSize, int pageIndex)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Coupon> coupons = db.Coupons.Include(c => c.Store).Where(c => c!.Store!.UserId == uid);
            try
            {   //Search
                if (s != null)
                {
                    coupons = SearchCoupon(s, coupons);
                }
                //Filter
                if (filter_Coupon != null && filter_Coupon == CouponStatusEnum.Active.ToString())
                {
                    coupons = FilterByActiveStatus(coupons);
                }
                else if (filter_Coupon != null && filter_Coupon == CouponStatusEnum.Inactive.ToString())
                {
                    coupons = FilterByInActiveStatus(coupons);
                }
                //Orderby
                if (order_by != null && order_by == CouponOrderByEnum.AscExpireDate.ToString())
                {
                    coupons = OrderByAscExpireDate(coupons);
                }
                else if (order_by != null && order_by == CouponOrderByEnum.DescExpireDate.ToString())
                {
                    coupons = OrderByDescExpireDate(coupons);
                }
                return coupons.Adapt<IEnumerable<StoreDashboardCoupon>>().Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        public int CountCouponPage(string uid, string? s, string? filter_Coupon)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Coupon> coupons = db.Coupons.Where(c => c!.Store!.UserId == uid);
            try
            {   //Search
                if (s != null)
                {
                    coupons = SearchCoupon(s, coupons);
                }
                //Filter
                if (filter_Coupon != null && filter_Coupon == CouponStatusEnum.Active.ToString())
                {
                    coupons = FilterByActiveStatus(coupons);
                }
                else if (filter_Coupon != null && filter_Coupon == CouponStatusEnum.Inactive.ToString())
                {
                    coupons = FilterByInActiveStatus(coupons);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return coupons.Count();
        }

        public async Task CreateCoupon(Coupon obj)
        {
            var db = new CakeCuriousDbContext();
            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (obj.Discount != null && obj.Discount > 0)
            {
                if (obj.DiscountType == (int)CouponDiscountTypeEnum.PercentOff)
                {
                    if (obj.Discount >= 1 && obj.Discount <= 50)
                    {
                        obj.Discount /= 100;
                    }
                    else if (obj.Discount <= (decimal)0.5)
                    {

                    }
                    else throw new Exception("Discount value of PercentOff discount type must less than or equal 50%");
                }
            }
            else throw new Exception("Discount value must greater than 0");
            if (obj.ExpiryDate <= today) throw new Exception("Expire date must greater than today");
            if (db.Coupons.Any(c => c.Code == obj!.Code!.Trim() && c.StoreId == obj.StoreId && c.Status == (int)CouponStatusEnum.Active && c.ExpiryDate > DateTime.Now))
                throw new Exception("This code is already exitst");
            db.Coupons.Add(obj);
            await db.SaveChangesAsync();
        }

        public async Task<Coupon?> DeleteCoupon(Guid guid)
        {
            Coupon? obj;
            try
            {
                obj = await GetById(guid);
                if (obj == null) throw new Exception("Coupons that need to delete does not exist");
                obj.Status = (int)CouponStatusEnum.Inactive;
                var db = new CakeCuriousDbContext();
                db.Entry<Coupon>(obj).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await db.SaveChangesAsync();
                return obj;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<Coupon?> GetById(Guid guid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Coupons.FirstOrDefaultAsync(x => x.Id == guid);
        }

        public async Task<SimpleCoupon?> GetByIdForWeb(Guid guid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Coupons.ProjectToType<SimpleCoupon>().FirstOrDefaultAsync(x => x.Id == guid);
        }

        public IEnumerable<SimpleCoupon> GetValidSimpleCouponsOfStoreForUser(Guid storeId, string userId, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Coupons
                .OrderByDescending(x => x.ExpiryDate)
                .Where(x => x.ExpiryDate > DateTime.Now)
                .Where(x => x.Status == (int)CouponStatusEnum.Active)
                .Where(x => x.StoreId == storeId)
                .Where(x => x.MaxUses != null ? x.Orders!.Count < x.MaxUses : true)
                .Where(x => !(x.Orders!.Any(y => y.UserId == userId)))
                .Skip(skip)
                .Take(take)
                .ProjectToType<SimpleCoupon>();
        }

        public async Task<SimpleCoupon?> GetSimpleCouponOfStoreByCode(Guid storeId, string code)
        {
            var db = new CakeCuriousDbContext();
            return await db.Coupons
                .AsNoTracking()
                .Where(x => x.StoreId == storeId
                    && x.Code == code
                    && x.ExpiryDate > DateTime.Now
                    && x.Status == (int)CouponStatusEnum.Active)
                .ProjectToType<SimpleCoupon>()
                .FirstOrDefaultAsync();
        }

        public async Task<SimpleCoupon?> GetActiveSimpleCouponOfStoreById(Guid id, Guid storeId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Coupons
                .AsNoTracking()
                .AsSplitQuery()
                .Where(x => x.Id == id)
                .Where(x => x.StoreId == storeId)
                .Where(x => x.ExpiryDate > DateTime.Now)
                .Where(x => x.Status == (int)CouponStatusEnum.Active)
                .Where(x => x.Store!.Status == (int)StoreStatusEnum.Active)
                .Where(x => x.Store!.User!.Status == (int)UserStatusEnum.Active)
                .ProjectToType<SimpleCoupon>()
                .FirstOrDefaultAsync();
        }

        public async Task UpdateCoupon(Coupon obj)
        {
            DateTime today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            var db = new CakeCuriousDbContext();
            if (obj.Discount != null && obj.Discount > 0)
            {
                if (obj.DiscountType == (int)CouponDiscountTypeEnum.PercentOff)
                {
                    if (obj.Discount >= 1 && obj.Discount <= 50)
                    {
                        obj.Discount /= 100;
                    }
                    else if (obj.Discount <= (decimal)0.5)
                    {

                    }
                    else throw new Exception("Discount value of PercentOff discount type must less than or equal 50%");
                }
            }
            else throw new Exception("Discount value must greater than 0");
            if (obj.ExpiryDate <= today) throw new Exception("Expire date must greater than today");
            if (db.Coupons.Any(c => c.Code == obj!.Code!.Trim() && c.StoreId == obj.StoreId && c.Status == (int)CouponStatusEnum.Active && c.ExpiryDate > DateTime.Now))
                throw new Exception("This code is already exitst");
            db.Entry<Coupon>(obj).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Coupon>> GetAllActiveCoupon()
        {
            var db = new CakeCuriousDbContext();
            return await db.Coupons.Where(c => c.Status == (int)CouponStatusEnum.Active).ToListAsync();
        }

        public async Task UpdateRange(Coupon[] coupons)
        {
            var db = new CakeCuriousDbContext();
            db.Coupons.UpdateRange(coupons);
            await db.SaveChangesAsync();
        }
    }
}
