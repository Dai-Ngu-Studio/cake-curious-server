using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Reports
{
    public class StaffDashboardReportPage
    {
        public IEnumerable<StaffDashboardReport>? Reports { get; set;}
        public int TotalPage { get; set;}
    }
}
