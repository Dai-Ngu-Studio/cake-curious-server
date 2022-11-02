using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.DashboardReports
{
    public class AdminDashboardBarChart
    {
        public IEnumerable<int>? CurrentYearUserReport { get; set; }
        public IEnumerable<int>? LastYearnUserReport { get; set; }
    }
}
