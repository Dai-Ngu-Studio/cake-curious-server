using Google.Analytics.Data.V1Beta;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.DashboardReports;
using System.Globalization;
using System.Text;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardReportsController : ControllerBase
    {
        private readonly IDashboardReportRepository dashboardReportRepository;
        private const string AnalyticsPropertyId = "331411032";
        public DashboardReportsController(IDashboardReportRepository _dashboardReportRepository)
        {
            dashboardReportRepository = _dashboardReportRepository;       
        }
        [HttpGet("Admin")]
        public async Task<ActionResult<AdminDashboardReport>> GetAdminDashboardReport()
        {
            AdminDashboardReport dbr = new AdminDashboardReport();
            dbr = await dashboardReportRepository.generateAdminReport();

            BetaAnalyticsDataClient client = new BetaAnalyticsDataClientBuilder
            {
                CredentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"),
            }.Build();
            DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            // Initialize request argument(s)
            RunReportRequest request = new RunReportRequest
            {
                Property = "properties/" + AnalyticsPropertyId,
                Dimensions = { new Dimension { Name = "unifiedScreenName" }, },
                Metrics = { new Metric { Name = "screenPageViews" }, },
                DateRanges = { new DateRange { StartDate = monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }, },
            };

            // Make the request
            var response = client.RunReport(request);

            var stringBuilder = new StringBuilder();
            foreach (Row row in response.Rows)
            {
                if (row.DimensionValues[0].Value.Contains("Grocery")) {
                    TableRowStoreVisit tbv = new TableRowStoreVisit();
                    tbv.StoreName = row.DimensionValues[0].Value;
                    tbv.Visitors = row.MetricValues[0].Value;
                    dbr!.TableStoreVisit!.Add(tbv);
                } 
            }
            //{ row.MetricValues[0].Value};
           
            return Ok(dbr);
        }
    }
}
