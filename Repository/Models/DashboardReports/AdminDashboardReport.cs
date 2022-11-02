using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.DashboardReports
{
    public class AdminDashboardReport
    {
        public AdminDashboardCardStats? CardStats { get; set; }
        public AdminDashboardBarChart? BarChart { get; set; }
        public AdminDashboardLineChart? LineChart { get; set; }
        public List<TableRowStoreVisit>? TableStoreVisit { get; set; } = new List<TableRowStoreVisit>();
        public IEnumerable<TableRowUserVisit>? TableUserVisits { get; set; }

    }
}
