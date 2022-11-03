﻿using Google.Analytics.Data.V1Beta;
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
            int month = 1;
            DateTime startAtMonday = DateTime.Now.AddDays(DayOfWeek.Monday - DateTime.Now.DayOfWeek);
            DateTime NewYear = new DateTime(DateTime.Now.Year, 1, 1);
            DateTime LastYear = new DateTime(DateTime.Now.AddYears(-1).Year, 1, 1);
            DateTime LastDateOfLastYear = new DateTime(DateTime.Now.AddYears(-1).Year, 12, 31);

            //Get this year active user for line chart
            AdminDashboardLineChart lc = new AdminDashboardLineChart();
            RunReportRequest requestThisYearActiveUser = new RunReportRequest
            {
                Property = "properties/" + AnalyticsPropertyId,
                Dimensions = { new Dimension { Name = "month" }, },
                Metrics = { new Metric { Name = "activeUsers" }, },
                DateRanges = { new DateRange { StartDate = NewYear.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }, },
                OrderBys = { new OrderBy { Dimension = new OrderBy.Types.DimensionOrderBy() { DimensionName = "month" }, Desc = false }, },

            };
            var currentYearActiveUser = client.RunReport(requestThisYearActiveUser);
            while (month < DateTime.Now.Month + 1)
            {
                if (month.ToString() == currentYearActiveUser.Rows[0].DimensionValues[0].Value)
                {
                    foreach (var row in currentYearActiveUser.Rows)
                    {
                        //dbr!.LineChart!.CurrentYearUserVisit!.Add(Int32.Parse(row.MetricValues[0].Value));
                        lc!.CurrentYearUserVisit!.Add(Int32.Parse(row.MetricValues[0].Value));
                    }
                }
                else
                {
                    //dbr!.LineChart!.CurrentYearUserVisit!.Add(0);
                    lc!.CurrentYearUserVisit!.Add(0);
                }
                month++;
            }

            //Get Last year active user for line chart
            month = 1;
            try
            {
                RunReportRequest requestLastYearActiveUser = new RunReportRequest
                {
                    Property = "properties/" + AnalyticsPropertyId,
                    Dimensions = { new Dimension { Name = "month" }, },
                    Metrics = { new Metric { Name = "activeUsers" }, },
                    DateRanges = { new DateRange { StartDate = LastYear.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = LastDateOfLastYear.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }, },
                    OrderBys = { new OrderBy { Dimension = new OrderBy.Types.DimensionOrderBy() { DimensionName = "month" }, Desc = false }, }
                };
                var lastYearActiveUser = client.RunReport(requestLastYearActiveUser);
                while (month < DateTime.Now.Month + 1)
                {
                    if (month.ToString() == lastYearActiveUser.Rows[0].DimensionValues[0].Value)
                    {
                        foreach (var row in lastYearActiveUser.Rows)
                        {
                            //dbr!.LineChart!.LastYearUserVisit!.Add(Int32.Parse(row.MetricValues[0].Value));
                            lc!.LastYearUserVisit!.Add(Int32.Parse(row.MetricValues[0].Value));
                        }
                    }
                    else
                    {
                        //dbr!.LineChart!.LastYearUserVisit!.Add(0);
                        lc!.LastYearUserVisit!.Add(0);
                    }
                    month++;
                }
            }
            catch (Exception) // cach error if the data of this last year is not available
            {
                while (month < 12)
                {

                    //dbr!.LineChart!.LastYearUserVisit!.Add(0);
                    lc!.LastYearUserVisit!.Add(0);
                    month++;
                }
                Console.WriteLine("No data for active user in the last year");
            }
            dbr.LineChart = lc;

            //Get weely Active user
            RunReportRequest requestLastWeekActiveUser = new RunReportRequest
            {
                Property = "properties/" + AnalyticsPropertyId,
                Metrics = { new Metric { Name = "activeUsers" }, },
                DateRanges = { new DateRange { StartDate = startAtMonday.AddDays(-7).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = startAtMonday.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }, },
            };
            RunReportRequest requestActiveUser = new RunReportRequest
            {
                Property = "properties/" + AnalyticsPropertyId,
                Metrics = { new Metric { Name = "activeUsers" }, },
                DateRanges = { new DateRange { StartDate = startAtMonday.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }, },
            };
            var activeUserRes = client.RunReport(requestActiveUser);
            var lastWeekActiveUserRes = client.RunReport(requestLastWeekActiveUser);

            //Add active user by week data to report
            dbr!.CardStats!.CurrentWeekActiveUser = Decimal.Parse(activeUserRes.Rows[0].MetricValues[0].Value);
            decimal lastWeekActiveUser = Decimal.Parse(lastWeekActiveUserRes.Rows[0].MetricValues[0].Value);
            double sinceLastWeekActiveUser = dbr!.CardStats!.CurrentWeekActiveUser > 0 && lastWeekActiveUser > 0 ? (double)((dbr!.CardStats!.CurrentWeekActiveUser - lastWeekActiveUser) / (dbr!.CardStats!.CurrentWeekActiveUser > lastWeekActiveUser ? dbr!.CardStats!.CurrentWeekActiveUser : lastWeekActiveUser)) : -1;
            dbr!.CardStats!.SinceLastWeekActiveUser = Math.Round(sinceLastWeekActiveUser, 2);

            // Get store visit by month
            RunReportRequest storeVisitReq = new RunReportRequest
            {
                Property = "properties/" + AnalyticsPropertyId,
                Dimensions = { new Dimension { Name = "unifiedScreenName" }, },
                Metrics = { new Metric { Name = "screenPageViews" }, },
                DateRanges = { new DateRange { StartDate = monthStart.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), EndDate = "today" }, },
            };
            // Make the request
            var storeVisitRes = client.RunReport(storeVisitReq);
            foreach (Row row in storeVisitRes.Rows)
            {
                if (row.DimensionValues[0].Value.Contains("Grocery"))
                {
                    dbr!.TableStoreVisit!.Add(new TableRowStoreVisit { StoreName = row.DimensionValues[0].Value, Visitors = row.MetricValues[0].Value });
                    /*TableRowStoreVisit tbv = new TableRowStoreVisit();
                    tbv.StoreName = row.DimensionValues[0].Value;
                    tbv.Visitors = row.MetricValues[0].Value;
                    dbr!.TableStoreVisit!.Add(tbv);*/
                }
            }
            return Ok(dbr);
        }
    }
}
