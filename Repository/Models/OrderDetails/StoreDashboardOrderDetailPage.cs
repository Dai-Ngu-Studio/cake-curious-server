using Repository.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.OrderDetails
{
    public class StoreDashboardOrderDetailPage
    {
        public StoreDashboardOrderDetail? orderDetail { get; set; }
        public int TotalPage { get; set; }
    }
}
