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
        public IEnumerable<Coupon> SearchCoupon(string? keyWord, IEnumerable<Coupon> coupons)
        {
            coupons = coupons.Where(p => p!.Name!.Contains(keyWord!)).ToList();
            return coupons;
        }
        public IEnumerable<Coupon> OrderByAscName(IEnumerable<Coupon> coupons)
        {

            return coupons.OrderBy(p => p.Name).ToList();
        }
        public IEnumerable<Coupon> OrderByDescName(IEnumerable<Coupon> coupons)
        {
            return coupons.OrderByDescending(p => p.Name).ToList();
        }
        public IEnumerable<Coupon> FilterByActiveStatus(IEnumerable<Coupon> coupons)
        {
            return coupons.Where(p => p.Status == (int)CouponStatusEnum.Active).ToList();
        }
        public IEnumerable<Coupon> FilterByInActiveStatus(IEnumerable<Coupon> coupons)
        {
            return coupons.Where(p => p.Status == (int)CouponStatusEnum.Active).ToList();
        }
        public IEnumerable<StoreDashboardCoupon>? GetCouponsOfAStore(string uid, string? s, string? order_by, string? filter_Coupon, int pageSize, int pageIndex)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Coupon> coupons = db.Coupons.Include(c => c.Store).Where(c => c!.Store!.UserId == uid).ToList();
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
                if (order_by != null && order_by == CouponOrderByEnum.AscName.ToString())
                {
                    coupons = OrderByAscName(coupons);
                }
                else if (order_by != null && order_by == CouponOrderByEnum.DescName.ToString())
                {
                    coupons = OrderByDescName(coupons);
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
        public int? CountCouponPage(string uid, string? s, string? order_by, string? filter_Coupon)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Coupon> coupons = db.Coupons.Include(c => c.Store).Where(c => c!.Store!.UserId == uid).ToList();
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
                if (order_by != null && order_by == CouponOrderByEnum.AscName.ToString())
                {
                    coupons = OrderByAscName(coupons);
                }
                else if (order_by != null && order_by == CouponOrderByEnum.DescName.ToString())
                {
                    coupons = OrderByDescName(coupons);
                }
                return coupons.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task CreateCoupon(Coupon obj)
        {
            var db = new CakeCuriousDbContext();
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

        public async Task<SimpleCoupon?> GetSimpleCouponOfStoreById(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Coupons
                .AsNoTracking()
                .Where(x => x.Id == id
                    && x.ExpiryDate > DateTime.Now
                    && x.Status == (int)CouponStatusEnum.Active)
                .ProjectToType<SimpleCoupon>()
                .FirstOrDefaultAsync();
        }

        public async Task UpdateCoupon(Coupon obj)
        {
            try
            {
                var db = new CakeCuriousDbContext();
                db.Entry<Coupon>(obj).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
