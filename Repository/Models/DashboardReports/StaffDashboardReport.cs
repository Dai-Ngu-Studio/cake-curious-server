using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.DashboardReports
{
    public class StaffDashboardReport
    {
        public StaffDashboardCardStats? CardStats { get; set; }
        public StaffDashboardBarChart? BarChart { get; set; }
        public StaffDashboardLineChart? LineChart { get; set; }
    }
}
