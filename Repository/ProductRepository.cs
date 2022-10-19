using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Products;
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
            return prods.Where(p => p.ProductType == (int)ProductTypeEnum.Ingredient).ToList();
        }
        public IEnumerable<Product> FilterByTool(IEnumerable<Product> prods)
        {
            return prods.Where(p => p.ProductType == (int)ProductTypeEnum.Tool).ToList();
        }
        public IEnumerable<Product> OrderByAscPrice(IEnumerable<Product> prods)
        {
            return prods.OrderBy(p => p.Price).ToList();
        }

        public IEnumerable<Product> OrderbyByDescPrice(IEnumerable<Product> prods)
        {

            return prods.OrderByDescending(p => p.Price).ToList();
        }

        public IEnumerable<Product> OrderByAscName(IEnumerable<Product> prods)
        {

            return prods.OrderBy(p => p.Name).ToList();
        }
        public IEnumerable<Product> OrderByDescName(IEnumerable<Product> prods)
        {
            return prods.OrderByDescending(p => p.Name).ToList();
        }

        public IEnumerable<Product> SearchProduct(string? keyWord, IEnumerable<Product> prods)
        {
            prods = prods.Where(p => p!.Name!.Contains(keyWord!)).ToList();
            return prods;
        }
        public IEnumerable<StoreDashboardProduct>? GetProducts(string? s, string? order_by, string? product_type, int pageIndex, int pageSize)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Product> prods = db.Products.ToList();
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
                return prods.Adapt<IEnumerable<StoreDashboardProduct>>().Skip((pageIndex - 1) * pageSize)
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

        public int CountDashboardProducts(string? s, string? order_by, string? product_type)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Product> prods = db.Products.ToList();
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
                return prods.Count();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return 0;
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
    }
}