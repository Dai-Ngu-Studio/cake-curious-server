using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repository.Models.Stores;
using Mapster;
using Repository.Interfaces;
using Repository.Constants.Products;
using Repository.Constants.Stores;

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
                else if (order_by != null && order_by == StoreSortEnum.DescName.ToString())
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

        public async Task<Store?> GetById(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Stores.FirstOrDefaultAsync(x => x.Id == id);
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
                if (filter_Store != null && filter_Store == "StatusActive")
                {
                    stores = FilterByStatusActive(stores);
                }
                else if (filter_Store != null && filter_Store == "StatusInActive")
                {
                    stores = FilterByStatusInactive(stores);
                }
                //Sort
                if (order_by != null && order_by == "DescName")
                {
                    stores = OrderByDescName(stores);
                }
                else if (order_by != null && order_by == "AscName")
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
    }
}
