using Repository.Models.Stores;
using Repository.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Orders
{
    public class StoreDashboardOrders
    {
        public Guid? Id { get; set; }
        public SimpleUser? User { get; set; }
        public SimpleStore? Store { get; set; }
        public decimal? Total { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

    }
}
