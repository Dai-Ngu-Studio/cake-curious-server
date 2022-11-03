using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.DashboardReports
{
    public class AdminDashboardBarChart
    {
        public List<int>? CurrentYearUserReport { get; set; } = new List<int>();
        public List<int>? LastYearUserReport { get; set; } = new List<int>();
    }
}
