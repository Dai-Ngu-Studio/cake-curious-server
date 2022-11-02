using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.DashboardReports
{
    public class AdminDashboardCardStats
    {
        public decimal CurrentWeekReport { get; set; }
        public double SinceLastWeekReport { get; set; }
        public decimal CurrentWeekNewBaker { get; set; }
        public double SinceLastMonthNewBaker   { get; set; } 
        public decimal CurrentWeekActiveUser { get; set; }
        public double SinceLastWeekActiveUser { get; set; }
        public decimal CurrentMonthNewStore { get; set; }
        public double SinceLastMonthNewStore   { get; set; }
    }
}
