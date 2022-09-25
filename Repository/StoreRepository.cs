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

        public IEnumerable<Store> FilterByAscName(IEnumerable<Store> stores)
        {
            return stores.OrderBy(p => p.Name).ToList();
        }
        public IEnumerable<Store> FilterByDescName(IEnumerable<Store> stores)
        {
            return stores.OrderByDescending(p => p.Name).ToList();
        }
        public IEnumerable<Store> FilterByStatusUnAvailable(IEnumerable<Store> stores)
        {
            return stores.Where(p => p.Status == 2).ToList();
        }

        public IEnumerable<Store> FilterByStatusMaintaining(IEnumerable<Store> stores)
        {
            return stores.Where(p => p.Status == 3).ToList();
        }
        public IEnumerable<Store> FilterByStatusAvailable(IEnumerable<Store> stores)
        {
            return stores.Where(p => p.Status == 1).ToList();
        }

        public IEnumerable<Store> SearchStore(string keyWord)
        {
            IEnumerable<Store> stores;
            var db = new CakeCuriousDbContext();
            stores = db.Stores.Where(p => p.Name.Contains(keyWord)).ToList();
            return stores;
        }

        public IEnumerable<Store> FindStore(string s, string fillter_Store)
        {
            IEnumerable<Store> result;
            IEnumerable<Store> store = SearchStore(s);
            try
            {
                if (fillter_Store == null) return store;
                else if (fillter_Store == "ByDescendingName")
                {
                    result = FilterByDescName(store);
                    return result;
                }
                else if (fillter_Store == "ByAscendingName")
                {
                    result = FilterByAscName(store);
                    return result;
                }
                else if (fillter_Store == "ByStatusMaintaining")
                {
                    result = FilterByStatusMaintaining(store);
                    return result;
                }
                else if (fillter_Store == "ByStatusUnaVailable")
                {
                    result = FilterByStatusUnAvailable(store);
                    return result;
                }
                else if (fillter_Store == "ByStatusAvailable")
                {
                    result = FilterByStatusAvailable(store);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<Store> GetById(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Stores.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<AdminDashboardStores>> GetStores(int pageSize, int pageIndex)
        {
            try
            {
                IEnumerable<AdminDashboardStores> Stores;
                var db = new CakeCuriousDbContext();
                Stores = await db.Stores
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ProjectToType<AdminDashboardStores>().ToListAsync();
                return Stores;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<Store> Delete(Guid id)
        {
            Store store = null;
            try
            {
                store = await GetById(id);
                if (store == null) throw new Exception("Store that need to delete does not exist");
                var db = new CakeCuriousDbContext();
                db.Stores.Remove(store);
                db.SaveChanges();
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
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
