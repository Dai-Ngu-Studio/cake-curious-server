using BusinessObject;
using Mapster;
using Repository.Models.Product;

namespace Repository.Configuration.Mappings
{
    public class ProductMapConfiguration
    {
        public static void RegisterProductMapping()
        {
            TypeAdapterConfig<Product, CartDetailProduct>
                .NewConfig()
                .Map(dest => dest.Ingredient, src => ((Dictionary<Guid, string>)MapContext.Current!.Parameters["productIngredients"]).Count > 0
                    ? ((Dictionary<Guid, string>)MapContext.Current!.Parameters["productIngredients"])[(Guid)src.Id!]
                    : string.Empty);
        }
    }
}
