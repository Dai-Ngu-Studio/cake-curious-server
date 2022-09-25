
using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.Product;
using Repository.Models.Recipes;
using Repository.Models.Users;
using Repository.Utilites;
using Mapster;

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

        public IEnumerable<Product> FilterByDecsPrice(IEnumerable<Product> prods)
        {
            return prods.OrderBy(p => p.Price).ToList();
        }

        public IEnumerable<Product> FilterByAscPrice(IEnumerable<Product> prods)
        {

            return prods.OrderByDescending(p => p.Price).ToList();
        }

        public IEnumerable<Product> FilterByAscName(IEnumerable<Product> prods)
        {

            return prods.OrderBy(p => p.Name).ToList();
        }
        public IEnumerable<Product> FilterByDescName(IEnumerable<Product> prods)
        {
            return prods.OrderByDescending(p => p.Name).ToList();
        }

        public IEnumerable<Product> FilterByStatusOutOfStock(IEnumerable<Product> prods)
        {
            return prods.Where(p => p.Status == 2).ToList();
        }
        public IEnumerable<Product> FilterByStatusInStock(IEnumerable<Product> prods)
        {
            return prods.Where(p => p.Status == 1).ToList();
        }

        public IEnumerable<Product> SearchProduct(string keyWord)
        {
            IEnumerable<Product> prods;
            var db = new CakeCuriousDbContext();
            prods = db.Products.Where(p => p.Name.Contains(keyWord)).ToList();
            return prods;
        }

        public IEnumerable<Product> FindProduct(string s, string fillter_product)
        {
            IEnumerable<Product> result;
            IEnumerable<Product> prod = SearchProduct(s);
            try
            {
                if (fillter_product == null) return prod;
                else if (fillter_product == "ByAscendingPrice")
                {
                    result = FilterByAscPrice(prod);
                    return result;
                }
                else if (fillter_product == "ByDescendingPrice")
                {
                    result = FilterByDecsPrice(prod);
                    return result;
                }
                else if (fillter_product == "ByDescendingName")
                {
                    result = FilterByDescName(prod);
                    return result;
                }
                else if (fillter_product == "ByAscendingName")
                {
                    result = FilterByAscName(prod);
                    return result;
                }
                else if (fillter_product == "ByOutOfStockStatus")
                {
                    result = FilterByStatusOutOfStock(prod);
                    return result;
                }
                else if (fillter_product == "ByInStockStatus")
                {
                    result = FilterByStatusInStock(prod);
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<Product> GetById(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Products.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<StoreDashboardProducts>> GetProducts(int pageSize, int pageIndex)
        {
            try
            {
                IEnumerable<StoreDashboardProducts> products;
                var db = new CakeCuriousDbContext();
                products = await db.Products
                                .Skip((pageIndex - 1) * pageSize)
                                .Take(pageSize).ProjectToType<StoreDashboardProducts>().ToListAsync();
                return products;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<Product> Delete(Guid id)
        {
            Product prod = null;
            try
            {
                prod = await GetById(id);
                if (prod == null) throw new Exception("Product that need to delete does not exist");
                var db = new CakeCuriousDbContext();
                db.Products.Remove(prod);
                db.SaveChanges();
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
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}