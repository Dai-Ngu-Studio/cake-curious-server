using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Models.Stores;
using Mapster;
using Repository.Interfaces;
using Repository.Constants.Products;
using Repository.Constants.Stores;
using Repository.Constants.Orders;

namespace Repository
{
    public class StoreRepository : IStoreRepository
    {
        public async Task Add(Store obj)
        {
            var db = new CakeCuriousDbContext();
            db.Stores.Add(obj);
            await db.SaveChangesAsync();
        }

        public IEnumerable<Store> OrderByAscName(IEnumerable<Store> stores)
        {
            return stores.OrderBy(p => p.Name).ToList();
        }
        public IEnumerable<Store> OrderByDescName(IEnumerable<Store> stores)
        {
            return stores.OrderByDescending(p => p.Name).ToList();
        }
        public IEnumerable<Store> FilterByStatusActive(IEnumerable<Store> stores)
        {
            return stores.Where(p => p.Status == (int)StoreStatusEnum.Active).ToList();
        }

        public IEnumerable<Store> FilterByStatusInactive(IEnumerable<Store> stores)
        {
            return stores.Where(p => p.Status == (int)StoreStatusEnum.Inactive).ToList();
        }

        public IEnumerable<Store> SearchStore(string? keyWord, IEnumerable<Store> stores)
        {

            return stores.Where(p => p.Name!.Contains(keyWord!)).ToList();
        }

        public IEnumerable<AdminDashboardStore>? GetStores(string? s, string? order_by, string? filter_Store, int pageSize, int pageIndex)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Store> stores = db.Stores.Include(s => s.User).ToList();
            try
            {
                //search
                if (s != null)
                {
                    stores = SearchStore(s, stores);
                }
                //filter
                if (filter_Store != null && filter_Store == StoreStatusEnum.Active.ToString())
                {
                    stores = FilterByStatusActive(stores);
                }
                else if (filter_Store != null && filter_Store == StoreStatusEnum.Inactive.ToString())
                {
                    stores = FilterByStatusInactive(stores);
                }
                //orderby
                if (order_by != null && order_by == StoreSortEnum.DescName.ToString())
                {
                    stores = OrderByDescName(stores);
                }
                else if (order_by != null && order_by == StoreSortEnum.AscName.ToString())
                {
                    stores = OrderByAscName(stores);
                }              
                return stores.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).Adapt<IEnumerable<AdminDashboardStore>>().ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }
        public async Task<Guid> getStoreIdByUid(string uid)
        {
            try
            {
                var db = new CakeCuriousDbContext();
                Store? result = await db.Stores.FirstOrDefaultAsync(s => s.UserId == uid);
                if (result == null) throw new Exception("Invalid store id.You need to create a store to create a product");
                return result!.Id!.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Guid.Empty;
        }
        public async Task<Store?> GetById(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Stores.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<int> DeleteAllOrderOfInActiveStore(Guid guid)
        {

            if (guid == Guid.Empty) { return -1; }
            else
            {
                var db = new CakeCuriousDbContext();
                IEnumerable<Order> orders = await db.Orders.Where(o => o.StoreId == guid).ToListAsync();
                foreach (var order in orders)
                {
                    if (order.Status == (int)OrderStatusEnum.Pending || order.Status == (int)OrderStatusEnum.Processing)
                    {
                        order.Status = (int)OrderStatusEnum.Cancelled;
                        order.CompletedDate = DateTime.Now;
                        db.Entry<Order>(order).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        await db.SaveChangesAsync();
                    }
                }
            }
            return 1;
        }
        public async Task<int> DeleteAllProductOfInActiveStore(Guid guid)
        {
            if (guid == Guid.Empty) { return -1; }
            else
            {
                var db = new CakeCuriousDbContext();
                IEnumerable<Product> products = await db.Products.Where(p => p.StoreId == guid).ToListAsync();
                foreach (var product in products)
                {
                    product.Status = (int)ProductStatusEnum.Inactive;
                    db.Entry<Product>(product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    await db.SaveChangesAsync();
                }
            }
            return 1;
        }
        public async Task<Store?> Delete(Guid? id)
        {
            Store? store = null;
            try
            {
                store = await GetById(id!.Value);
                if (store == null) throw new Exception("Store that need to delete does not exist");
                store.Status = (int)StoreStatusEnum.Inactive;
                var db = new CakeCuriousDbContext();
                db.Entry<Store>(store).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await db.SaveChangesAsync();
                int deleteOrderStatus = await DeleteAllOrderOfInActiveStore(id.Value);
                if (deleteOrderStatus < 0) throw new Exception("Delete Store Success .But Delete Store's orders fail");
                int deleteProductStatus = await DeleteAllProductOfInActiveStore(id.Value);
                if (deleteProductStatus < 0) throw new Exception("Delete Store Success .But Delete Product's orders fail");
                return store;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task Update(Store updateObj)
        {
            try
            {
                var db = new CakeCuriousDbContext();
                db.Entry<Store>(updateObj).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public int CountDashboardStores(string? s, string? order_by, string? filter_Store)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Store> stores = db.Stores.ToList();
            try
            {
                //search
                if (s != null)
                {
                    stores = SearchStore(s, stores);
                }
                //filter
                if (filter_Store != null && filter_Store == StoreStatusEnum.Active.ToString())
                {
                    stores = FilterByStatusActive(stores);
                }
                else if (filter_Store != null && filter_Store == StoreStatusEnum.Inactive.ToString())
                {
                    stores = FilterByStatusInactive(stores);
                }
                //Sort
                if (order_by != null && order_by == StoreSortEnum.DescName.ToString())
                {
                    stores = OrderByDescName(stores);
                }
                else if (order_by != null && order_by == StoreSortEnum.AscName.ToString())
                {
                    stores = OrderByAscName(stores);
                }
                return stores.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }

        public async Task<bool> IsStoreExisted(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Stores.AsNoTracking().AnyAsync(x => x.Id == id && x.Status == (int)StoreStatusEnum.Active);
        }

        public Task<StoreDetail?> GetByUserId(string? uid)
        {
            var db = new CakeCuriousDbContext();
            return db.Stores.Include(s => s.User!).ProjectToType<StoreDetail>().FirstOrDefaultAsync(x => x.UserId == uid);
        }

        public async Task<DetailStore?> GetStoreDetails(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Stores
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectToType<DetailStore>()
                .FirstOrDefaultAsync();
        }
    }
}
