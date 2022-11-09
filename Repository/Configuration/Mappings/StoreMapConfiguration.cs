using BusinessObject;
using Mapster;
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
        }
    }
}
