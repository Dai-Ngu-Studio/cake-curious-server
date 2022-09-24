using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using Mapster;
using Repository.Models.Orders;

namespace Repository.Configuration.Mappings
{
    public static class OrderMapConfiguration
    {
        public static void RegisterOrderMapping()
        {
            TypeAdapterConfig<Order, StoreDashboardOrders>
                .NewConfig();
                
           
        }
    }
}
