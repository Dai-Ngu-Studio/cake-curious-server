
using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Products;
using Repository.Interfaces;
using Repository.Models.Product;
using Repository.Models.Recipes;
using Repository.Models.Users;
using Repository.Utilites;

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

        public IEnumerable<StoreDashboardProduct> FilterByDecsPrice(IEnumerable<StoreDashboardProduct> prods)
        {
            return prods.OrderBy(p => p.Price).ToList();
        }

        public IEnumerable<StoreDashboardProduct> FilterByAscPrice(IEnumerable<StoreDashboardProduct> prods)
        {

            return prods.OrderByDescending(p => p.Price).ToList();
        }

        public IEnumerable<StoreDashboardProduct> FilterByAscName(IEnumerable<StoreDashboardProduct> prods)
        {

            return prods.OrderBy(p => p.Name).ToList();
        }
        public IEnumerable<StoreDashboardProduct> FilterByDescName(IEnumerable<StoreDashboardProduct> prods)
        {
            return prods.OrderByDescending(p => p.Name).ToList();
        }

        public IEnumerable<StoreDashboardProduct> FilterByInactiveStatus(IEnumerable<StoreDashboardProduct> prods)
        {
            return prods.Where(p => p.Status == (int)ProductStatusEnum.Inactive).ToList();
        }
        public IEnumerable<StoreDashboardProduct> FilterByActiveStatus(IEnumerable<StoreDashboardProduct> prods)
        {
            return prods.Where(p => p.Status == (int)ProductStatusEnum.Active).ToList();
        }

        public IEnumerable<StoreDashboardProduct> SearchProduct(string keyWord)
        {
            IEnumerable<StoreDashboardProduct> prods;
            var db = new CakeCuriousDbContext();
            prods = db.Products.Where(p => p.Name!.Contains(keyWord)).ProjectToType<StoreDashboardProduct>().ToList();
            return prods;
        }

        public IEnumerable<StoreDashboardProduct>? GetProducts(string? s, string? filter_product, int pageIndex, int pageSize)
        {
            IEnumerable<StoreDashboardProduct> result;
            IEnumerable<StoreDashboardProduct> prod = SearchProduct(s!);
            try
            {
                if (filter_product == null)
                    return prod.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                else if (filter_product == "ByAscendingPrice")
                {
                    result = FilterByAscPrice(prod);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
                else if (filter_product == "ByDescendingPrice")
                {
                    result = FilterByDecsPrice(prod);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
                else if (filter_product == "ByDescendingName")
                {
                    result = FilterByDescName(prod);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
                else if (filter_product == "ByAscendingName")
                {
                    result = FilterByAscName(prod);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
                else if (filter_product == "ByActiveStatus")
                {
                    result = FilterByActiveStatus(prod);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
                else if (filter_product == "ByInStockStatus")
                {
                    result = FilterByInactiveStatus(prod);
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

        public int CountDashboardProducts(string? s, string? filter_product)
        {
            IEnumerable<StoreDashboardProduct> result;
            IEnumerable<StoreDashboardProduct> prod = SearchProduct(s!);
            try
            {
                if (filter_product == null)
                    return prod.Count();
                else if (filter_product == "ByAscendingPrice")
                {
                    result = FilterByAscPrice(prod);
                    return result.Count();
                }
                else if (filter_product == "ByDescendingPrice")
                {
                    result = FilterByDecsPrice(prod);
                    return result.Count();
                }
                else if (filter_product == "ByDescendingName")
                {
                    result = FilterByDescName(prod);
                    return result.Count();
                }
                else if (filter_product == "ByAscendingName")
                {
                    result = FilterByAscName(prod);
                    return result.Count();
                }
                else if (filter_product == "ByInactiveStatus")
                {
                    result = FilterByInactiveStatus(prod);
                    return result.Count();
                }
                else if (filter_product == "ByActiveStatus")
                {
                    result = FilterByActiveStatus(prod);
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