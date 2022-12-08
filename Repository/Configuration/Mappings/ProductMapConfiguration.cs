using BusinessObject;
using Mapster;
using Repository.Models.Product;

namespace Repository.Configuration.Mappings
{
    public class ProductMapConfiguration
    {
        public static void RegisterProductMapping()
        {
            TypeAdapterConfig<Product, BundleDetailProduct>
                .NewConfig()
                .Map(dest => dest.Ingredient, src => ((Dictionary<Guid, string>)MapContext.Current!.Parameters["productIngredients"])[(Guid)src.Id!]);

            TypeAdapterConfig<Product, DetailProduct>
                .NewConfig()
                .Map(dest => dest.RatingCount, src => src.OrderDetails!.Where(x => x.Rating != null).Count());
        }
    }
}
