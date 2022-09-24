
using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
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

     
        public async Task<IEnumerable<Product>> FilterByDecsPrice()
        {
            var db = new CakeCuriousDbContext();

            return  db.Products.OrderBy(p => p.Price).ToList();
        }
        public async Task<IEnumerable<Product>> FilterByAscPrice()
        {
            var db = new CakeCuriousDbContext();

            return  db.Products.OrderByDescending(p => p.Price).ToList();
        }
        public async Task<IEnumerable<Product>> FilterByAscName()
        {
            var db = new CakeCuriousDbContext();

            return  db.Products.OrderBy(p => p.Name).ToList();
        }
        public async Task<IEnumerable<Product>> FilterByDescName()
        {
            var db = new CakeCuriousDbContext();

            return  db.Products.OrderByDescending(p => p.Name).ToList();
        }
        public async Task<IEnumerable<Product>> FilterByStatusOutOfStock()
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<Product> prod = await db.Products.Where(p => p.Status == 2).ToListAsync();

            return prod;
        }
        public async Task<IEnumerable<Product>> FilterByStatusInStock()
        {
            var db = new CakeCuriousDbContext();

            return  db.Products.Where(p => p.Status == 1).ToList();
        }
        public async Task<IEnumerable<Product>> FilterProduct(string keyWord)
        {
            IEnumerable<Product> prod = null;

            try
            {
                if (keyWord == null) throw new Exception("Keyword for filter is null");
                else if(keyWord == "ByAscendingPrice")
                {
                    prod = await FilterByAscPrice();
                    return  prod;
                }
                else if (keyWord == "ByDescendingPrice")
                {
                    prod =  await FilterByDecsPrice();
                    return prod;
                }
                else if (keyWord == "ByDescendingName")
                {
                    prod = await FilterByDescName();
                    return prod;
                }
                else if (keyWord == "ByAscendingName")
                {
                    prod = await FilterByAscName();
                    return prod;
                }
                else if (keyWord == "ByOutOfStockStatus")
                {
                    prod = await FilterByStatusOutOfStock();
                    return prod;
                }
                else if (keyWord == "ByInStockStatus")
                {
                    prod = await FilterByStatusInStock();
                    return prod;
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

        public async Task<IEnumerable<Product>> GetProducts(int pageSize, int? pageIndex)

        {
            try
            {
                
                PaginatedList<Product> products;
                var db = new CakeCuriousDbContext();
                IQueryable<Product> productIQ = from s in db.Products
                                                select s;
                products = await PaginatedList<Product>.CreateAsync(
                    productIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
                return products.ToList();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message) ;
            }
            return null;
        }
        /*public async Task<IEnumerable<Product>> GetProducts()
        {
            var db = new CakeCuriousDbContext();
            return await db.Products.ToListAsync();
        }*/

       

        public async Task<IEnumerable<Product>> SearchProduct(string keyWord)
        {
            var db = new CakeCuriousDbContext();
            return  db.Products.Where(p => p.Name.Contains(keyWord)).ToList();
        }
        public async Task Delete(Guid id)
        {
            Product prod = null;
            try
            {
                prod = await GetById(id);
                if (prod == null) throw new Exception("Product that need to delete does not exist");
                var db = new CakeCuriousDbContext();
                db.Products.Remove(prod);
                db.SaveChanges();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }
        public async Task Update(Product updateObj)
        {
            Product prod = null;
            try
            {
                prod = await GetById(updateObj.Id.Value);
                if (prod == null) throw new Exception("Product that need to update does not exist");
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