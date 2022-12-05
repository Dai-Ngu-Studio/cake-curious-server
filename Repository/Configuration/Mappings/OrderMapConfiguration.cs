using BusinessObject;
using Mapster;
using Repository.Models.Orders;

namespace Repository.Configuration.Mappings
{
    public static class OrderMapConfiguration
    {
        public static void RegisterOrderMapping()
        {
            TypeAdapterConfig<Order, InfoOrder>
                .NewConfig()
                .Map(dest => dest.PhotoUrl, src => src.OrderDetails!.First().Product!.PhotoUrl)
                .Map(dest => dest.Products, src => src.OrderDetails!.Count);

            TypeAdapterConfig<Order, DetailOrder>
                .NewConfig()
                .Map(dest => dest.Products, src => src.OrderDetails!.Count);
        }
    }
}
