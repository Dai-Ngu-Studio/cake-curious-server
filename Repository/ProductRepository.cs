using BusinessObject;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Products;
using Repository.Constants.Users;
using Repository.Interfaces;
using Repository.Models.Product;

namespace Repository
{
    public class ProductRepository : IProductRepository
    {
        public async Task Add(Product obj)
        {
            var db = new CakeCuriousDbContext();
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

        public async Task Update(Product updateObj)
        {
            try
            {
                var db = new CakeCuriousDbContext();
                db.Entry<Product>(updateObj).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public int CountDashboardProducts(Guid? id, string? s, string? order_by, string? product_type)
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
                else if (order_by != null && order_by == ProductOrderByEnum.DescName.ToString())
                {
                    prods = OrderbyByDescPrice(prods);
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

        public async Task<Product?> GetActiveProductReadonly(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Status == (int)ProductStatusEnum.Active);
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
                ? $"select top {take} [p].[id], [p].[product_type], [p].[name], [p].[price], [p].[discount], [p].[photo_url], abs(checksum([p].id, rand(@randSeed)*rand(@randSeed))) as [key] from [Product] as [p] left join [Store] as [s] on [p].[store_id] = [s].[id] left join [User] as [u] on [s].[user_id] = [u].[id] where abs(checksum([p].id, rand(@randSeed)* rand(@randSeed))) > @key and ([p].[product_type] = @productType) and ([p].[status] = @productStatus) and ([s].[status] = @storeStatus) and ([u].[status] = @userStatus) and ([s].[id] = @storeId) order by abs(checksum([p].id, rand(@randSeed) * rand(@randSeed)))"
                : $"select top {take} [p].[id], [p].[product_type], [p].[name], [p].[price], [p].[discount], [p].[photo_url], abs(checksum([p].id, rand(@randSeed)*rand(@randSeed))) as [key] from [Product] as [p] left join [Store] as [s] on [p].[store_id] = [s].[id] left join [User] as [u] on [s].[user_id] = [u].[id]  where abs(checksum([p].id, rand(@randSeed)* rand(@randSeed))) > @key and ([p].[product_type] = @productType) and ([p].[status] = @productStatus) and ([s].[status] = @storeStatus) and ([u].[status] = @userStatus) order by abs(checksum([p].id, rand(@randSeed) * rand(@randSeed)))";
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
                        Discount = (decimal)reader["discount"],
                        PhotoUrl = (string)reader["photo_url"],
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
                .ProjectToType<GroceryProduct>()
                .ToListAsync();
        }

        public async Task<ICollection<CartOrder>> GetCartOrders(List<Guid> storeIds, List<Guid> productIds)
        {
            using (var scope = new MapContextScope())
            {
                scope.Context.Parameters.Add("productIds", productIds);

                var db = new CakeCuriousDbContext();
                return await db.Stores
                    .AsNoTracking()
                    .Where(x => storeIds.Any(y => y == (Guid)x.Id!))
                    .ProjectToType<CartOrder>()
                    .ToListAsync();
            }
        }
    }
}