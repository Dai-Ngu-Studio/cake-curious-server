using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.DashboardReports
{
    public class AdminDashboardLineChart
    {
        public List<int>? CurrentYearUserVisit { get; set; } = new List<int>();
        public List<int>? LastYearUserVisit { get; set; } = new List<int>();
    }
}
