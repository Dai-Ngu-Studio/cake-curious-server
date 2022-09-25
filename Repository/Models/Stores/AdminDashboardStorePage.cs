using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Stores
{
    public class AdminDashboardStorePage
    {
        public int? TotalPage { get; set; }
        public IEnumerable<AdminDashboardStore>? Stores { get; set; }
    }
}
