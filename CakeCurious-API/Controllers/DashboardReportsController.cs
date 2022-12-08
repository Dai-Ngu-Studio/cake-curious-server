using Google.Analytics.Data.V1Beta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Repository.Models.DashboardReports;
using System.Globalization;
using System.Security.Claims;

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
            DateTime startAtSunday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(DayOfWeek.Sunday - DateTime.Now.DayOfWeek);
            DateTime LastYear = new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1);
            AdminDashboardLineChart lc = new AdminDashboardLineChart();
            try
            {    //Get this year active user for line chart cost  1.89s 
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Error when get monthly active users data from google analytic");
            }

            int startMonth = -1;
            int endMonth = -1;
            //find startMonth in CurrentYearUnprocessedReports
            for (int i = 0; i < lc.LastYearActiveUser.Count(); i++)
            {
                if (lc.LastYearActiveUser[i] != 0)
                {
                    startMonth = i;
                    break;
                }
            }
            //find endMonth in CurrentYearUnprocessedReports
            for (int i = lc.LastYearActiveUser.Count() - 1; i >= 0; --i)
            {
                if (lc.CurrentYearActiveUser[i] != 0)
                {
                    endMonth = i;
                    break;
                }
            }
            //find startMonth in CurrentYearProcessedReports
            for (int i = 0; i < lc.CurrentYearActiveUser.Count(); i++)
            {
                if (lc.CurrentYearActiveUser[i] != 0)
                {
                    if (startMonth >= 0 && startMonth > i) startMonth = i;
                    else startMonth = i;
                    break;
                }


            }
            //find endMonth in CurrentYearProcessedReports
            for (int i = lc.CurrentYearActiveUser.Count() - 1; i >= 0; --i)
            {
                if (lc.LastYearActiveUser[i] != 0)
                {
                    if (endMonth < i) endMonth = i;
                    break;
                }
            }
            lc.CurrentYearActiveUser = lc.CurrentYearActiveUser.GetRange(startMonth, endMonth - startMonth + 1);
            lc.LastYearActiveUser = lc.LastYearActiveUser.GetRange(startMonth, endMonth - startMonth + 1);
            lc.Month = lc.Month.GetRange(startMonth, endMonth - startMonth + 1);
            dbr.LineChart = lc;

            try
            {

                //Get weely Active user cost 1.x s
                RunReportRequest requestActiveUser = new RunReportRequest
                {
                    Property = "properties/" + AnalyticsPropertyId,
                    Dimensions = { new Dimension { Name = "week" }, },
                    Metrics = { new Metric { Name = "activeUsers" }, },
                    DateRanges = { new DateRange { StartDate = startAtSunday.AddDays(-7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = "today" }, },
                };
                //Add active user by week data to report
                var activeUserRes = await client.RunReportAsync(requestActiveUser);
                dbr!.CardStats!.CurrentWeekActiveUser = activeUserRes.Rows.Count() > 1 ? Int32.Parse(activeUserRes.Rows[1].MetricValues[0].Value) : 0;
                decimal lastWeekActiveUser = activeUserRes.Rows.Count() > 1 ? Int32.Parse(activeUserRes.Rows[0].MetricValues[0].Value) : Int32.Parse(activeUserRes.Rows[0].MetricValues[0].Value);
                double sinceLastWeekActiveUser = (double)((dbr!.CardStats!.CurrentWeekActiveUser - lastWeekActiveUser) / (dbr!.CardStats!.CurrentWeekActiveUser > lastWeekActiveUser ? dbr!.CardStats!.CurrentWeekActiveUser : lastWeekActiveUser));
                dbr!.CardStats!.SinceLastWeekActiveUser = Math.Round(sinceLastWeekActiveUser, 2);

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                return BadRequest("Error when get weekly active users from google analytic");
            }


            try
            {
                // Get store visit by month cost about 2.6s
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Error when get monthly store visit in google analytics");
            }
            return Ok(dbr);
        }
        [HttpGet("Staff")]
        [Authorize]

        public async Task<ActionResult<StaffDashboardStatistic>> GetStaffDashboardReport()
        {
            StaffDashboardStatistic dbr;
            dbr = await dashboardReportRepository.generateStaffReport();
            return dbr;
        }
        [HttpGet("Store")]
        [Authorize]
        public async Task<ActionResult<AdminDashboardReport>> GetStoreDashboardReport()
        {
            StoreDashboardReport dbr;
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? storeId = await storeRepository.getStoreIdByUid(uid!);
            dbr = await dashboardReportRepository.generateStoreReport(storeId.Value);
            DateTime startAtSunday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(DayOfWeek.Sunday - DateTime.Now.DayOfWeek);
            BetaAnalyticsDataClient client = new BetaAnalyticsDataClientBuilder
            {
                CredentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS"),
            }.Build();
            try
            {
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Error when get current week store visit in google analytics");
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Error when get yearly store visit in google analytics");
            }
            int startMonth = -1;
            int endMonth = -1;
            //find startMonth in LastYearStoreVisit
            for (int i = 0; i < lc.LastYearStoreVisit.Count(); i++)
            {
                if (lc.LastYearStoreVisit[i] != 0)
                {
                    startMonth = i;
                    break;
                }

            }
            //find endMonth in LastYearStoreVisit
            for (int i = lc.LastYearStoreVisit.Count() - 1; i >= 0; --i)
            {
                if (lc.CurrentYearStoreVisit[i] != 0)
                {
                    endMonth = i;
                    break;
                }
            }
            //find startMonth in CurrentYearStoreVisit
            for (int i = 0; i < lc.CurrentYearStoreVisit.Count(); i++)
            {
                if (lc.CurrentYearStoreVisit[i] != 0)
                {
                    if (startMonth >= 0 && startMonth > i) startMonth = i;
                    else startMonth = i;
                    break;
                }

            }
            //find endMonth in CurrentYearStoreVisit
            for (int i = lc.CurrentYearStoreVisit.Count() - 1; i >= 0; --i)
            {
                if (lc.LastYearStoreVisit[i] != 0)
                {
                    if (endMonth < i) endMonth = i;
                    break;
                }
            }
            Console.WriteLine(startMonth + " " + endMonth);
            lc.CurrentYearStoreVisit = lc.CurrentYearStoreVisit.GetRange(startMonth, endMonth - startMonth + 1);
            lc.LastYearStoreVisit = lc.LastYearStoreVisit.GetRange(startMonth, endMonth - startMonth + 1);
            lc.Month = lc.Month.GetRange(startMonth, endMonth - startMonth + 1);
            dbr.LineChart = lc;
            return Ok(dbr);
        }
    }
}
