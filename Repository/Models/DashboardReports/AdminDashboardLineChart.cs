using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.DashboardReports
{
    public class AdminDashboardLineChart
    {
        public List<int> CurrentYearStoreVisit { get; set; } = new List<int>();
        public List<int> LastYearStoreVisit { get; set; } = new List<int>();
    }
}
