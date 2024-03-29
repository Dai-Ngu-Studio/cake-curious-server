﻿using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Repository.Models.Stores;
using Mapster;
using Repository.Interfaces;
using Repository.Constants.Products;
using Repository.Constants.Stores;
using Repository.Constants.Orders;
using Repository.Constants.Users;
using Microsoft.Data.SqlClient;
using BusinessObject.FunctionMappings;

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

        public IEnumerable<Store> OrderByAscCreatedDate(IEnumerable<Store> stores)
        {
            return stores.OrderBy(p => p.CreatedDate);
        }
        public IEnumerable<Store> OrderByDescCreatedDate(IEnumerable<Store> stores)
        {
            return stores.OrderByDescending(p => p.CreatedDate);
        }
        public IEnumerable<Store> FilterByStatusActive(IEnumerable<Store> stores)
        {
            return stores.Where(p => p.Status == (int)StoreStatusEnum.Active);
        }

        public IEnumerable<Store> FilterByStatusInactive(IEnumerable<Store> stores)
        {
            return stores.Where(p => p.Status == (int)StoreStatusEnum.Inactive);
        }

        public IEnumerable<Store> SearchStore(string? keyWord, IEnumerable<Store> stores)
        {

            return stores.Where(p => p.Name!.ToLower().Contains(keyWord!.ToLower()));
        }

        public IEnumerable<Store> GetStoresAfter(int take, Guid? lastGuid)
        {
            var db = new CakeCuriousDbContext();
            if (lastGuid != null)
            {
                return db.Stores
                    .OrderBy(x => x.Id)
                    .Where(x => ((Guid)x.Id!).IsGreaterThan((Guid)lastGuid!))
                    .Take(take);
            }
            else
            {
                return db.Stores.OrderBy(x => x.Id).Take(take);
            }
        }

        public IEnumerable<AdminDashboardStore>? GetStores(string? s, string? order_by, string? filter_Store, int pageSize, int pageIndex)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Store> stores = db.Stores.Include(s => s.User).OrderByDescending(s => s.CreatedDate);
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
                if (order_by != null && order_by == StoreSortEnum.DescCreatedDate.ToString())
                {
                    stores = OrderByDescCreatedDate(stores);
                }
                else if (order_by != null && order_by == StoreSortEnum.AscCreatedDate.ToString())
                {
                    stores = OrderByAscCreatedDate(stores);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return stores.Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).Adapt<IEnumerable<AdminDashboardStore>>().ToList();
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

        public async Task<SimpleStore?> GetNameByIdReadonly(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Stores.AsNoTracking().ProjectToType<SimpleStore>().FirstOrDefaultAsync(x => x.Id == id);
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
                if (updateObj.Status != null && updateObj.Status == (int)StoreStatusEnum.Inactive)
                {
                    int deleteOrderStatus = await DeleteAllOrderOfInActiveStore(updateObj!.Id!.Value);
                    if (deleteOrderStatus < 0) throw new Exception("Delete Store Success .But Delete Store's orders fail");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task UpdateRange(IEnumerable<Store> stores)
        {
            var db = new CakeCuriousDbContext();
            db.Stores.UpdateRange(stores);
            await db.SaveChangesAsync();
        }

        public async Task<decimal> GetRatingForStore(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Products
                .Where(x => x.Rating > 0)
                .Where(x => x.StoreId == id)
                .AverageAsync(x => x.Rating) ?? 0;
        }

        public int CountDashboardStores(string? s, string? filter_Store)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Store> stores = db.Stores;
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
                return stores.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }

        public async Task<CartStore?> GetReadonlyCartStore(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Stores
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectToType<CartStore>()
                .FirstOrDefaultAsync();
        }

        public async Task<StoreDetail?> GetByUserId(string? uid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Stores.Include(s => s.User!).ProjectToType<StoreDetail>().FirstOrDefaultAsync(x => x.UserId == uid);
        }
        public async Task<AdminDashboardStore?> GetStoreDetailForWeb(Guid guid)
        {
            var db = new CakeCuriousDbContext();
            return await db.Stores.Include(s => s.User!).ProjectToType<AdminDashboardStore>().FirstOrDefaultAsync(x => x.Id == guid);
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

        public async Task<Store> CreateStoreForUser(User user, Store store)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                db.Stores.Add(store);
                user.StoreId = store.Id;
                db.Users.Update(user);
                await db.SaveChangesAsync();

                await transaction.CommitAsync();
                return store;
            }
        }

        public async Task<ICollection<GroceryStore>> GetSuggestedStores(List<Guid> storeIds)
        {
            var db = new CakeCuriousDbContext();
            return await db.Stores
                .AsNoTracking()
                .Where(x => storeIds.Any(y => y == (Guid)x.Id!))
                .Where(x => x.Status == (int)StoreStatusEnum.Active)
                .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                .ProjectToType<GroceryStore>()
                .ToListAsync();
        }

        public async Task<ICollection<ActiveCouponsStore>> GetActiveCouponsStore(string userId, int skip, int take)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("userId", userId);

                var db = new CakeCuriousDbContext();
                return await db.Stores
                    .AsNoTracking()
                    .OrderByDescending(x => x.Rating)
                    .Where(x => x.Coupons!.Count > 0)
                    .Where(x => x.Status == (int)StoreStatusEnum.Active)
                    .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                    .Skip(skip)
                    .Take(take)
                    .ProjectToType<ActiveCouponsStore>()
                    .ToListAsync();
            }
        }

        public async Task<ICollection<GroceryStore>> Explore(int randSeed, int take, int key)
        {
            var result = new List<GroceryStore>();
            var db = new CakeCuriousDbContext();
            string query = $"select top {take} [s].[id], [s].[name], [s].[rating], [s].[photo_url], abs(checksum([s].[id], rand(@randSeed)*rand(@randSeed))) as [key] from [Store] as [s] left join [User] as [u] on [s].[user_id] = [u].[id] where abs(checksum([s].[id], rand(@randSeed)*rand(@randSeed))) > @key and ([s].[status] = @storeStatus) and ([u].[status] = @userStatus) order by abs(checksum([s].[id], rand(@randSeed)*rand(@randSeed)))";
            var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = query;
            var userStatus = (int)UserStatusEnum.Active;
            var storeStatus = (int)StoreStatusEnum.Active;
            cmd.Parameters.Add(new SqlParameter("@take", take));
            cmd.Parameters.Add(new SqlParameter("@randSeed", randSeed));
            cmd.Parameters.Add(new SqlParameter("@key", key));
            cmd.Parameters.Add(new SqlParameter("@storeStatus", storeStatus));
            cmd.Parameters.Add(new SqlParameter("@userStatus", userStatus));
            if (cmd.Connection!.State != System.Data.ConnectionState.Open)
            {
                await cmd.Connection.OpenAsync();
            }
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    result.Add(new GroceryStore
                    {
                        Id = (Guid)reader["id"],
                        Name = (string)reader["name"],
                        PhotoUrl = reader["photo_url"].GetType() == typeof(string) ? (string)reader["photo_url"] : null,
                        Rating = (decimal)reader["rating"],
                        Key = (int)reader["key"],
                    });
                }
            }
            if (cmd.Connection!.State == System.Data.ConnectionState.Open)
            {
                await cmd.Connection!.CloseAsync();
            }
            await db.DisposeAsync();
            return result;
        }

        public async Task<bool> IsStoreExisted(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Stores.AsNoTracking().AnyAsync(x => x.Id == id);
        }
    }
}
