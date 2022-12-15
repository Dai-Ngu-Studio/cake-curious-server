using BusinessObject;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Products;
using Repository.Constants.Users;
using Repository.Interfaces;
using Repository.Models.Orders;
using Repository.Models.Product;

namespace Repository
{
    public class ProductRepository : IProductRepository
    {
        public async Task Add(Product obj)
        {
            var db = new CakeCuriousDbContext();
            if (obj.Price <= 0) throw new Exception("Product price or quantity must greater than 0");
            if (obj.Quantity < 0) throw new Exception("Product quantity must greater than or equal 0");
            db.Products.Add(obj);
            await db.SaveChangesAsync();
        }
        public IEnumerable<Product> FilterByIngredient(IEnumerable<Product> prods)
        {
            return prods.Where(p => p.ProductType == (int)ProductTypeEnum.Ingredient);
        }
        public IEnumerable<Product> FilterByTool(IEnumerable<Product> prods)
        {
            return prods.Where(p => p.ProductType == (int)ProductTypeEnum.Tool);
        }
        public IEnumerable<Product> OrderByAscPrice(IEnumerable<Product> prods)
        {
            return prods.OrderBy(p => p.Price);
        }

        public IEnumerable<Product> OrderbyByDescPrice(IEnumerable<Product> prods)
        {

            return prods.OrderByDescending(p => p.Price);
        }

        public IEnumerable<Product> OrderByAscName(IEnumerable<Product> prods)
        {

            return prods.OrderBy(p => p.Name);
        }
        public IEnumerable<Product> OrderByDescName(IEnumerable<Product> prods)
        {
            return prods.OrderByDescending(p => p.Name);
        }

        public IEnumerable<Product> SearchProduct(string? keyWord, IEnumerable<Product> prods)
        {
            prods = prods.Where(p => p!.Name!.ToLower().Contains(keyWord!.ToLower()));
            return prods;
        }
        public IEnumerable<StoreProductDetail>? GetProducts(Guid? id, string? s, string? order_by, string? product_type, int pageIndex, int pageSize)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Product> prods = db.Products.Include(p => p.ProductCategory).Where(p => p.StoreId == id);
            try
            {   //Search
                if (s != null)
                {
                    prods = SearchProduct(s, prods);
                }
                //Filter
                if (product_type != null && product_type == ProductTypeEnum.Ingredient.ToString())
                {
                    prods = FilterByIngredient(prods);
                }
                else if (product_type != null && product_type == ProductTypeEnum.Tool.ToString())
                {
                    prods = FilterByTool(prods);
                }
                //Orderby
                if (order_by != null && order_by == ProductOrderByEnum.AscName.ToString())
                {
                    prods = OrderByAscName(prods);
                }
                else if (order_by != null && order_by == ProductOrderByEnum.DescName.ToString())
                {
                    prods = OrderByDescName(prods);
                }
                else if (order_by != null && order_by == ProductOrderByEnum.AscPrice.ToString())
                {
                    prods = OrderByAscPrice(prods);
                }
                else if (order_by != null && order_by == ProductOrderByEnum.DescPrice.ToString())
                {
                    prods = OrderbyByDescPrice(prods);
                }
                return prods.Adapt<IEnumerable<StoreProductDetail>>().Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<StoreProductDetail?> GetByIdForStore(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Products.Include(p => p.ProductCategory).ProjectToType<StoreProductDetail>().FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<Product?> GetById(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Products.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<Product?> Delete(Guid id)
        {
            Product? prod;
            try
            {
                prod = await GetById(id);
                if (prod == null) throw new Exception("Product that need to delete does not exist");
                prod.Status = (int)ProductStatusEnum.Inactive;
                var db = new CakeCuriousDbContext();
                db.Entry<Product>(prod).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await db.SaveChangesAsync();
                return prod;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task Update(Product product)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead))
            {
                await db.Database.ExecuteSqlRawAsync($"select [p].[quantity] from [Product] as [p] with (updlock) where [p].[id] = '{product.Id}'");
                product.Rating = (await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == product.Id))!.Rating;
                await db.Database.ExecuteSqlRawAsync("update [p] " +
                    "set [p].[name] = {0} " +
                    ", [p].[description] = {1} " +
                    ", [p].[photo_url] = {2} " +
                    ", [p].[quantity] = [p].[quantity] + {3} " +
                    ", [p].[price] = {4} " +
                    ", [p].[product_type] = {5} " +
                    ", [p].[status] = {6} " +
                    ", [p].[category_id] = {7} " +
                    ", [p].[last_updated] = {8} " +
                    "from [Product] as [p] with (updlock) " +
                    "where [p].[id] = {9}", 
                    product.Name!, product.Description!, product.PhotoUrl!, 
                    product.Quantity!, product.Price!, product.ProductType!, 
                    product.Status!, product.ProductCategoryId!, DateTime.Now, product.Id!);
                await db.SaveChangesAsync();
                await transaction.CommitAsync(); // Commit transaction, remove lock
            }
        }

        public int CountDashboardProducts(Guid? id, string? s, string? product_type)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Product> prods = db.Products.Where(p => p.StoreId == id);
            try
            {   //Search
                if (s != null)
                {
                    prods = SearchProduct(s, prods);
                }
                //Filter
                if (product_type != null && product_type == ProductTypeEnum.Ingredient.ToString())
                {
                    prods = FilterByIngredient(prods);
                }
                else if (product_type != null && product_type == ProductTypeEnum.Tool.ToString())
                {
                    prods = FilterByTool(prods);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return prods.Count();
        }

        public async Task<Product?> GetProductReadonly(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Product?> GetActiveProductOfStoreReadonly(Guid id, Guid storeId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Products
                .AsNoTracking()
                .AsSplitQuery()
                .Where(x => x.Id == id)
                .Where(x => x.StoreId == storeId)
                .Where(x => x.Status == (int)ProductStatusEnum.Active)
                .Where(x => x.Store!.Status == (int)StoreStatusEnum.Active)
                .Where(x => x.Store!.User!.Status == (int)UserStatusEnum.Active)
                .ProjectToType<Product>()
                .FirstOrDefaultAsync();
        }

        public async Task<DetailProduct?> GetProductDetails(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Products
                .AsNoTracking()
                .Where(x => x.Id == id)
                .ProjectToType<DetailProduct>()
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<GroceryProduct>> Explore(int productType, int randSeed, int take, int key, Guid? storeId)
        {
            var result = new List<GroceryProduct>();
            var db = new CakeCuriousDbContext();
            string query = (storeId != null)
                ? $"select top {take} [p].[id], [p].[product_type], [p].[name], [p].[price], [p].[photo_url], [p].[rating], abs(checksum([p].id, rand(@randSeed)*rand(@randSeed))) as [key] from [Product] as [p] left join [Store] as [s] on [p].[store_id] = [s].[id] left join [User] as [u] on [s].[user_id] = [u].[id] where abs(checksum([p].id, rand(@randSeed)* rand(@randSeed))) > @key and ([p].[product_type] = @productType) and ([p].[status] = @productStatus) and ([s].[status] = @storeStatus) and ([u].[status] = @userStatus) and ([s].[id] = @storeId) order by abs(checksum([p].id, rand(@randSeed) * rand(@randSeed)))"
                : $"select top {take} [p].[id], [p].[product_type], [p].[name], [p].[price], [p].[photo_url], [p].[rating], abs(checksum([p].id, rand(@randSeed)*rand(@randSeed))) as [key] from [Product] as [p] left join [Store] as [s] on [p].[store_id] = [s].[id] left join [User] as [u] on [s].[user_id] = [u].[id] where abs(checksum([p].id, rand(@randSeed)* rand(@randSeed))) > @key and ([p].[product_type] = @productType) and ([p].[status] = @productStatus) and ([s].[status] = @storeStatus) and ([u].[status] = @userStatus) order by abs(checksum([p].id, rand(@randSeed) * rand(@randSeed)))";
            var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = query;
            var productStatus = (int)ProductStatusEnum.Active;
            var storeStatus = (int)StoreStatusEnum.Active;
            var userStatus = (int)UserStatusEnum.Active;
            cmd.Parameters.Add(new SqlParameter("@take", take));
            cmd.Parameters.Add(new SqlParameter("@randSeed", randSeed));
            cmd.Parameters.Add(new SqlParameter("@key", key));
            cmd.Parameters.Add(new SqlParameter("@productType", productType));
            cmd.Parameters.Add(new SqlParameter("@productStatus", productStatus));
            cmd.Parameters.Add(new SqlParameter("@storeStatus", storeStatus));
            cmd.Parameters.Add(new SqlParameter("@userStatus", userStatus));
            if (storeId != null)
            {
                cmd.Parameters.Add(new SqlParameter("@storeId", storeId));
            }
            if (cmd.Connection!.State != System.Data.ConnectionState.Open)
            {
                await cmd.Connection.OpenAsync();
            }
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (reader.Read())
                {
                    result.Add(new GroceryProduct
                    {
                        Id = (Guid)reader["id"],
                        Name = (string)reader["name"],
                        ProductType = (int)reader["product_type"],
                        Price = (decimal)reader["price"],
                        PhotoUrl = (string)reader["photo_url"],
                        Rating = (decimal)reader["rating"],
                        Key = (int)reader["key"],
                    });
                }
            }
            if (cmd.Connection!.State == System.Data.ConnectionState.Open)
            {
                await cmd.Connection!.CloseAsync();
            }
            return result;
        }

        public async Task<ICollection<GroceryProduct>> GetSuggestedProducts(List<Guid> productIds)
        {
            var db = new CakeCuriousDbContext();
            return await db.Products
                .AsNoTracking()
                .Where(x => productIds.Any(y => y == (Guid)x.Id!))
                .Where(x => x.Status == (int)ProductStatusEnum.Active)
                .Where(x => x.Store!.Status == (int)StoreStatusEnum.Active)
                .Where(x => x.Store!.User!.Status == (int)UserStatusEnum.Active)
                .ProjectToType<GroceryProduct>()
                .ToListAsync();
        }

        public async Task<ICollection<CartOrder>> GetCartOrders(List<Guid> storeIds, List<Guid> productIds, List<Guid?> couponIds)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("productIds", productIds);
                scope.Context.Parameters.Add("couponIds", couponIds);

                var db = new CakeCuriousDbContext();
                return await db.Stores
                    .AsNoTracking()
                    .Where(x => storeIds.Any(y => y == (Guid)x.Id!))
                    .ProjectToType<CartOrder>()
                    .ToListAsync();
            }
        }

        public async Task<ICollection<BundleOrder>> GetBundles(List<Guid> storeIds, List<Guid> productIds, Dictionary<Guid, string> productIngredients)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("productIds", productIds);
                scope.Context.Parameters.Add("productIngredients", productIngredients);

                var db = new CakeCuriousDbContext();
                return await db.Stores
                    .AsNoTracking()
                    .Where(x => storeIds.Any(y => y == (Guid)x.Id!))
                    .Where(x => x.Status == (int)StoreStatusEnum.Active)
                    .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                    .ProjectToType<BundleOrder>()
                    .ToListAsync();
            }
        }
    }
}