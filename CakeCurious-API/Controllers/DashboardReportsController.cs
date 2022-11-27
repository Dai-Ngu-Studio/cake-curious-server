using Google.Analytics.Data.V1Beta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.DashboardReports;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardReportsController : ControllerBase
    {
        private readonly IDashboardReportRepository dashboardReportRepository;
        private readonly IStoreRepository storeRepository;
        private const string AnalyticsPropertyId = "331411032";
        public DashboardReportsController(IDashboardReportRepository _dashboardReportRepository, IStoreRepository _storeRepository)
        {
            dashboardReportRepository = _dashboardReportRepository;
            storeRepository = _storeRepository;
        }
        [HttpGet("Admin")]
        [Authorize]

        public async Task<ActionResult<AdminDashboardReport>> GetAdminDashboardReport()
        {
            AdminDashboardReport dbr;
            dbr = dashboardReportRepository.generateAdminReport();
            BetaAnalyticsDataClient client = new BetaAnalyticsDataClientBuilder
            {
                CredentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"),
            }.Build();
            DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime startAtSunday = DateTime.Now.AddDays(DayOfWeek.Sunday - DateTime.Now.DayOfWeek);
            DateTime LastYear = new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1);
            //Get this year active user for line chart(Not cost much time 1.89)
            AdminDashboardLineChart lc = new AdminDashboardLineChart();
            RunReportRequest requestThisYearActiveUser = new RunReportRequest
            {
                Property = "properties/" + AnalyticsPropertyId,
                Dimensions = { new Dimension { Name = "month" }, new Dimension { Name = "year" } },
                Metrics = { new Metric { Name = "activeUsers" }, },
                DateRanges = { new DateRange { StartDate = LastYear.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = "today" }, },
                OrderBys = { new OrderBy { Dimension = new OrderBy.Types.DimensionOrderBy() { DimensionName = "month" }, Desc = false }, }
            };
            var currentYearActiveUser = await client.RunReportAsync(requestThisYearActiveUser);
            foreach (var row in currentYearActiveUser.Rows)
            {
                if (Int32.Parse(row.DimensionValues[1].Value) == DateTime.Now.Year)
                    lc!.CurrentYearActiveUser[Int32.Parse(row.DimensionValues[0].Value) - 1] = Int32.Parse(row.MetricValues[0].Value);
                else
                    lc!.LastYearActiveUser[Int32.Parse(row.DimensionValues[0].Value) - 1] = Int32.Parse(row.MetricValues[0].Value);
            }
            dbr.LineChart = lc;

            //Get weely Active user 1.x s
            RunReportRequest requestActiveUser = new RunReportRequest
            {
                Property = "properties/" + AnalyticsPropertyId,
                Dimensions = { new Dimension { Name = "week" }, },
                Metrics = { new Metric { Name = "activeUsers" }, },
                DateRanges = { new DateRange { StartDate = startAtSunday.AddDays(-7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = "today" }, },
            };

            //Add active user by week data to report
            var activeUserRes = await client.RunReportAsync(requestActiveUser);
            dbr!.CardStats!.CurrentWeekActiveUser = activeUserRes.Rows.Count() > 1 ? Int32.Parse(activeUserRes.Rows[0].MetricValues[0].Value) : 0;
            decimal lastWeekActiveUser = activeUserRes.Rows.Count() > 1 ? Int32.Parse(activeUserRes.Rows[1].MetricValues[0].Value) : Int32.Parse(activeUserRes.Rows[0].MetricValues[0].Value);
            double sinceLastWeekActiveUser = (double)((dbr!.CardStats!.CurrentWeekActiveUser - lastWeekActiveUser) / (dbr!.CardStats!.CurrentWeekActiveUser > lastWeekActiveUser ? dbr!.CardStats!.CurrentWeekActiveUser : lastWeekActiveUser));
            dbr!.CardStats!.SinceLastWeekActiveUser = Math.Round(sinceLastWeekActiveUser, 2);

            // Get store visit by month 2.6s
            RunReportRequest storeVisitReq = new RunReportRequest
            {
                Property = "properties/" + AnalyticsPropertyId,
                Dimensions = { new Dimension { Name = "unifiedScreenName" }, },
                Metrics = { new Metric { Name = "screenPageViews" }, },
                DateRanges = { new DateRange { StartDate = monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = "today" }, },
            };
            // Make the request
            var storeVisitRes = await client.RunReportAsync(storeVisitReq);
            foreach (Row row in storeVisitRes.Rows.Where(r => r.DimensionValues[0].Value.Contains("StoreDemoData")))
            {
                dbr!.TableStoreVisit!.Add(new TableRowStoreVisit { StoreName = row.DimensionValues[0].Value.Substring(row.DimensionValues[0].Value.LastIndexOf("/") + 1), Visitors = row.MetricValues[0].Value });
            }
            return Ok(dbr);
        }
        [HttpGet("Store")]
        [Authorize]
        public async Task<ActionResult<AdminDashboardReport>> GetStoreDashboardReport()
        {
            StoreDashboardReport dbr;
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? storeId = await storeRepository.getStoreIdByUid(uid!);
            dbr = await dashboardReportRepository.generateStoreReport(storeId.Value);
            DateTime startAtSunday = DateTime.Now.AddDays(DayOfWeek.Sunday - DateTime.Now.DayOfWeek);
            BetaAnalyticsDataClient client = new BetaAnalyticsDataClientBuilder
            {
                CredentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"),
            }.Build();
            //CardStat StoreVisit
            // Get store visit by week
            RunReportRequest storeVisitReq = new RunReportRequest
            {
                Property = "properties/" + AnalyticsPropertyId,
                Dimensions = { new Dimension { Name = "unifiedScreenName" }, },
                Metrics = { new Metric { Name = "screenPageViews" }, },
                DateRanges = { new DateRange { StartDate = startAtSunday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = "today" }, },
            };
            // Make the request
            var storeVisitRes = await client.RunReportAsync(storeVisitReq);
            foreach (Row row in storeVisitRes.Rows)
            {
                if (row.DimensionValues[0].Value.Contains("StoreDemoData/" + storeId.Value.ToString()))
                {
                    dbr!.CardStats!.CurrentWeekStoreVisit = Int32.Parse(row.MetricValues[0].Value);
                }
            }
            //LineChart
            StoreDashboardLineChart lc = new StoreDashboardLineChart();
            DateTime LastYear = new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1);
            try
            {
                RunReportRequest requestMonthlyStoreVisit = new RunReportRequest
                {
                    Property = "properties/" + AnalyticsPropertyId,
                    Dimensions = { new Dimension { Name = "unifiedScreenName" }, new Dimension { Name = "month" }, new Dimension { Name = "year" } },
                    Metrics = { new Metric { Name = "screenPageViews" }, },
                    DateRanges = { new DateRange { StartDate = LastYear.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = "today" }, },
                    OrderBys = { new OrderBy { Dimension = new OrderBy.Types.DimensionOrderBy() { DimensionName = "month" }, Desc = false }, },

                };
                var resMonthlyStoreVisit = await client.RunReportAsync(requestMonthlyStoreVisit);
                foreach (Row row in resMonthlyStoreVisit.Rows)
                {
                    if (Int32.Parse(row.DimensionValues[2].Value) == DateTime.Now.Year)
                    {
                        if (row.DimensionValues[0].Value.Contains("StoreDemoData/" + storeId.Value.ToString()))
                        {
                            lc!.CurrentYearStoreVisit![Int32.Parse(row.DimensionValues[1].Value) - 1] = Int32.Parse(row.MetricValues[0].Value);
                        }
                    }
                    else
                    {
                        if (row.DimensionValues[0].Value.Contains("StoreDemoData/" + storeId.Value.ToString()))
                        {
                            lc!.LastYearStoreVisit![Int32.Parse(row.DimensionValues[1].Value) - 1] = Int32.Parse(row.MetricValues[0].Value);
                        }
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("No data for active user in the last year");
            }
            dbr.LineChart = lc;
            return Ok(dbr);
        }
    }
}
