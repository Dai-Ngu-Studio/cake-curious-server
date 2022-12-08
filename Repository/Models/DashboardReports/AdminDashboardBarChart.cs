using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.DashboardReports
{
    public class AdminDashboardBarChart
    {
        public List<int>? CurrentMonthReport { get; set; } = new List<int>() { 0, 0, 0, 0 };
        public List<int>? LastMonthReport { get; set; } = new List<int>() { 0, 0, 0, 0 };
        public List<string> Week { get; set; } = new List<string> { "Week1", "Week2", "Week3", "Week4" };

    }
}
