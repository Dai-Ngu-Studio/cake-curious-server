﻿using BusinessObject;
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

        public IEnumerable<AdminDashboardStore> FilterByAscName(IEnumerable<AdminDashboardStore> stores)
        {
            return stores.OrderBy(p => p.Name).ToList();
        }
        public IEnumerable<AdminDashboardStore> FilterByDescName(IEnumerable<AdminDashboardStore> stores)
        {
            return stores.OrderByDescending(p => p.Name).ToList();
        }
        public IEnumerable<AdminDashboardStore> FilterByStatusActive(IEnumerable<AdminDashboardStore> stores)
        {
            return stores.Where(p => p.Status == (int)StoreStatusEnum.Active).ToList();
        }

        public IEnumerable<AdminDashboardStore> FilterByStatusInactive(IEnumerable<AdminDashboardStore> stores)
        {
            return stores.Where(p => p.Status == (int)StoreStatusEnum.Inactive).ToList();
        }

        public IEnumerable<AdminDashboardStore> SearchStore(string? keyWord)
        {
            IEnumerable<AdminDashboardStore> stores;
            var db = new CakeCuriousDbContext();
            stores = keyWord != null
                ? db.Stores.Where(p => p.Name!.Contains(keyWord!)).ProjectToType<AdminDashboardStore>().ToList()
                : db.Stores.ProjectToType<AdminDashboardStore>().ToList();
            return stores;
        }

        public IEnumerable<AdminDashboardStore>? GetStores(string? s, string? fillter_Store, int pageSize, int pageIndex)
        {
            IEnumerable<AdminDashboardStore> result;
            IEnumerable<AdminDashboardStore> store = SearchStore(s!);
            try
            {
                if (fillter_Store == null)
                    return store.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ToList();
                else if (fillter_Store == "ByDescendingName")
                {
                    result = FilterByDescName(store);
                    return result.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ToList();
                }
                else if (fillter_Store == "ByAscendingName")
                {
                    result = FilterByAscName(store);
                    return result.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ToList();
                }
                else if (fillter_Store == "ByStatusActive")
                {
                    result = FilterByStatusActive(store);
                    return result.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ToList();
                }
                else if (fillter_Store == "ByStatusInactive")
                {
                    result = FilterByStatusInactive(store);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
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

        public int CountDashboardStores(string? s, string? filter_Store)
        {
            IEnumerable<AdminDashboardStore> result;
            IEnumerable<AdminDashboardStore> store = SearchStore(s!);
            try
            {
                if (filter_Store == null)
                    return store.Count();
                else if (filter_Store == "ByDescendingName")
                {
                    result = FilterByDescName(store);
                    return result.Count();
                }
                else if (filter_Store == "ByAscendingName")
                {
                    result = FilterByAscName(store);
                    return result.Count();
                }

                else if (filter_Store == "ByStatusInactive")
                {
                    result = FilterByStatusInactive(store);
                    return result.Count();
                }
                else if (filter_Store == "ByStatusActive")
                {
                    result = FilterByStatusActive(store);
                    return result.Count();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }
    }
}
