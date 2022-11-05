using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.DashboardReports
{
    public class StoreDashboardReport
    {
        public StoreDashboardCardStats? CardStats { get; set; }
        public StoreDashboardBarChart? BarChart { get; set; }
        public StoreDashboardLineChart? LineChart { get; set; }
    }
}
