using BusinessObject;
using Mapster;
using Repository.Models.Product;
using Repository.Models.Stores;

namespace Repository.Configuration.Mappings
{
    public static class StoreMapConfiguration
    {
        public static void RegisterStoreMapping()
        {
            TypeAdapterConfig<Store, DetailStore>
                .NewConfig()
                .Map(dest => dest.Products, src => src.Products!.Count);

            TypeAdapterConfig<Store, CartOrder>
                .NewConfig()
                .Map(dest => dest.Products, src => src.Products!
                    .Where(x => ((IEnumerable<Guid>)MapContext.Current!.Parameters["productIds"])
                        .Any(y => y == (Guid)x.Id!) && (Guid)x.StoreId! == (Guid)src.Id!))
                .Map(dest => dest.Store, src => src);
        }
    }
}
