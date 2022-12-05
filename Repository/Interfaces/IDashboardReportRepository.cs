using Repository.Models.DashboardReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IDashboardReportRepository
    {
        public AdminDashboardReport generateAdminReport();
        public Task<StoreDashboardReport> generateStoreReport(Guid storeId);
        public Task<StaffDashboardReport> generateStaffReport();

    }
}
