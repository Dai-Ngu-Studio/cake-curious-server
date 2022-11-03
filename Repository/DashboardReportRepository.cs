using BusinessObject;
using Repository.Interfaces;
using Repository.Models.DashboardReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace Repository
{
    public class DashboardReportRepository : IDashboardReportRepository
    {
        public async Task<AdminDashboardReport> generateAdminReport()
        {
            AdminDashboardReport report = new AdminDashboardReport();
            var db = new CakeCuriousDbContext();
            AdminDashboardCardStats cs = new AdminDashboardCardStats();

            //new report by week
            DateTime startAtMonday = DateTime.Now.AddDays(DayOfWeek.Monday - DateTime.Now.DayOfWeek);
            cs.CurrentWeekReport = await db.ViolationReports.Where(r => r.SubmittedDate >= startAtMonday && r.SubmittedDate <= DateTime.Now).CountAsync();
            decimal lastWeekReport = await db.ViolationReports.Where(r => r.SubmittedDate >= startAtMonday.AddDays(-7) && r.SubmittedDate <= startAtMonday.AddDays(-1)).CountAsync();
            double sinceLastWeekReport = cs.CurrentWeekReport > 0 && lastWeekReport > 0 ? (double)((cs.CurrentWeekReport - lastWeekReport) / (cs.CurrentWeekReport > lastWeekReport ? cs.CurrentWeekReport : lastWeekReport)) : -1;
            cs.SinceLastWeekReport = sinceLastWeekReport;

            //new baker by month
            cs.CurrentWeekNewBaker = await db.Users.Where(u => u!.CreatedDate!.Value.Month == DateTime.Now.Month).CountAsync();
            decimal LastMonthNewUser = await db.Users.Where(u => u!.CreatedDate!.Value.Month == DateTime.Now.AddMonths(-1).Month).CountAsync();
            double sinceLastMonthNewUser = LastMonthNewUser > 0 && cs.CurrentWeekNewBaker > 0 ? (double)((cs.CurrentWeekNewBaker - LastMonthNewUser) / (cs.CurrentWeekNewBaker > LastMonthNewUser ? cs.CurrentWeekNewBaker : LastMonthNewUser)) : -1;
            cs.SinceLastMonthNewBaker = sinceLastMonthNewUser;

            //new store by month
            cs.CurrentMonthNewStore = await db.Stores.Where(s => s!.CreatedDate!.Value.Month == DateTime.Now.Month).CountAsync();
            decimal LastMonthNewStore = await db.Stores.Where(s => s!.CreatedDate!.Value.Month == DateTime.Now.AddMonths(-1).Month).CountAsync();
            double sinceLastMonthNewStore = cs.CurrentMonthNewStore > 0 && LastMonthNewStore > 0 ? (double)((cs.CurrentMonthNewStore - LastMonthNewStore) / (cs.CurrentMonthNewStore > LastMonthNewStore ? cs.CurrentMonthNewStore : LastMonthNewStore)) : -1;
            cs.SinceLastMonthNewStore = sinceLastMonthNewStore;
            report.CardStats = cs;

            //Add linechart
            AdminDashboardBarChart bc = new AdminDashboardBarChart();
            int firstMonth = 1;
            int lastYearMonth = 1;
            while (firstMonth < 13)
            {
                bc!.CurrentYearUserReport!.Add(await db.ViolationReports.Where(r => r!.SubmittedDate!.Value.Month == firstMonth & r.SubmittedDate.Value.Year == DateTime.Now.Year).CountAsync() > 0 ? await db.ViolationReports.Where(r => r!.SubmittedDate!.Value.Month == firstMonth & r.SubmittedDate.Value.Year == DateTime.Now.Year).CountAsync() : 0);
                firstMonth++;
            }
            Console.WriteLine();
            List<int> lastYearUserVisit = new List<int>();
            while (lastYearMonth < 13)
            {
                bc!.LastYearUserReport!.Add(await db.ViolationReports.Where(r => r!.SubmittedDate!.Value.Month == firstMonth & r.SubmittedDate.Value.Year == DateTime.Now.AddYears(-1).Year).CountAsync());
                lastYearMonth++;
            }
            report.BarChart = bc;

            //Get famous recipe for staticstic          
            foreach (var recipe in await db.Recipes.Include(r => r.Likes).Include(r => r.Comments).Include(r => r.User).OrderByDescending(r => r.Likes.Count()).OrderByDescending(r => r.Comments.Count()).Take(5).ToListAsync())
            {
                report!.TableFamousRecipe!.Add(new TableRowFamousRecipes
                {
                    Recipe = recipe.Name,
                    User = recipe!.User!.DisplayName,
                    Comments = recipe!.Comments!.Count(),
                    Likes = recipe!.Likes!.Count()
                });
            }
            return report;
        }
    }
}
