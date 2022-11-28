using BusinessObject;
using Repository.Interfaces;
using Repository.Models.DashboardReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Orders;

namespace Repository
{
    public class DashboardReportRepository : IDashboardReportRepository
    {
        public AdminDashboardReport generateAdminReport()
        {
            AdminDashboardReport report = new AdminDashboardReport();
            var db = new CakeCuriousDbContext();
            AdminDashboardCardStats cs = new AdminDashboardCardStats();
            //new report by week cost not much
            DateTime startAtSunday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(DayOfWeek.Sunday - DateTime.Now.DayOfWeek).Day);
            IEnumerable<ViolationReport> TwoLastWeekReport = db.ViolationReports.Where(r => r.SubmittedDate >= startAtSunday.AddDays(-7) && r.SubmittedDate <= DateTime.Now);
            cs.CurrentWeekReport = TwoLastWeekReport.Where(r => r.SubmittedDate >= startAtSunday && r.SubmittedDate <= DateTime.Now).Count();
            decimal lastWeekReport = TwoLastWeekReport.Where(r => r.SubmittedDate >= startAtSunday.AddDays(-7) && r.SubmittedDate <= startAtSunday.AddDays(-1)).Count();
            double sinceLastWeekReport = cs.CurrentWeekReport > 0 || lastWeekReport > 0 ? (double)((cs.CurrentWeekReport - lastWeekReport) / (cs.CurrentWeekReport > lastWeekReport ? cs.CurrentWeekReport : lastWeekReport)) : 0;
            cs.SinceLastWeekReport = Math.Round(sinceLastWeekReport, 2);

            //new baker by month
            IEnumerable<User> TwoLastMonthNewBaker = db.Users.Where(u => u!.CreatedDate!.Value.Month == DateTime.Now.Month || u!.CreatedDate!.Value.Month == DateTime.Now.AddMonths(-1).Month);
            cs.CurrentMonthNewBaker = TwoLastMonthNewBaker.Where(u => u!.CreatedDate!.Value.Month == DateTime.Now.Month).Count();
            decimal LastMonthNewUser = TwoLastMonthNewBaker.Where(u => u!.CreatedDate!.Value.Month == DateTime.Now.AddMonths(-1).Month).Count();
            double sinceLastMonthNewUser = LastMonthNewUser > 0 || cs.CurrentMonthNewBaker > 0 ? (double)((cs.CurrentMonthNewBaker - LastMonthNewUser) / (cs.CurrentMonthNewBaker > LastMonthNewUser ? cs.CurrentMonthNewBaker : LastMonthNewUser)) : 0;
            cs.SinceLastMonthNewBaker = Math.Round(sinceLastMonthNewUser, 2);

            //new store by month
            IEnumerable<Store> TwoLastMonthNewStore = db.Stores.Where(u => u!.CreatedDate!.Value.Month == DateTime.Now.Month || u!.CreatedDate!.Value.Month == DateTime.Now.AddMonths(-1).Month);
            cs.CurrentMonthNewStore = TwoLastMonthNewStore.Where(s => s!.CreatedDate!.Value.Month == DateTime.Now.Month).Count();
            decimal LastMonthNewStore = TwoLastMonthNewStore.Where(s => s!.CreatedDate!.Value.Month == DateTime.Now.AddMonths(-1).Month).Count();
            double sinceLastMonthNewStore = cs.CurrentMonthNewStore > 0 || LastMonthNewStore > 0 ? (double)((cs.CurrentMonthNewStore - LastMonthNewStore) / (cs.CurrentMonthNewStore > LastMonthNewStore ? cs.CurrentMonthNewStore : LastMonthNewStore)) : 0;
            cs.SinceLastMonthNewStore = Math.Round(sinceLastMonthNewStore, 2);
            report.CardStats = cs;

            //Add barchart 1.5s
            AdminDashboardBarChart bc = new AdminDashboardBarChart();
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;
            int lastMonth = DateTime.Now.AddMonths(-1).Month;
            int dateOfCurrentMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            int dateOfLastMonth = DateTime.DaysInMonth(currentYear, lastMonth);
            DateTime startOfCurrentMonthWeek1 = new DateTime(currentYear, currentMonth, 1);
            DateTime endOfCurrentMonthWeek1 = new DateTime(currentYear, currentMonth, 7);

            DateTime startOfCurrentMonthWeek2 = new DateTime(currentYear, currentMonth, 8);
            DateTime endOfCurrentMonthWeek2 = new DateTime(currentYear, currentMonth, 15);

            DateTime startOfCurrentMonthWeek3 = new DateTime(currentYear, currentMonth, 16);
            DateTime endOfCurrentMonthWeek3 = new DateTime(currentYear, currentMonth, 23);

            DateTime startOfCurrentMonthWeek4 = new DateTime(currentYear, currentMonth, 24);
            DateTime endOfCurrentMonthWeek4 = new DateTime(currentYear, currentMonth, dateOfCurrentMonth);
            DateTime endOfLastMonthWeek4 = new DateTime(currentYear, lastMonth, dateOfLastMonth);
            //Get Current Week reports
            IEnumerable<ViolationReport> ViolationReports = db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek1.AddMonths(-1) && r!.SubmittedDate! <= endOfCurrentMonthWeek4);
            int current1stWeekReports = ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek1 && r!.SubmittedDate! <= endOfCurrentMonthWeek1).Count();
            int current2ndWeekReports = ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek2 && r!.SubmittedDate! <= endOfCurrentMonthWeek2).Count();
            int current3rdWeekReports = ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek3 && r!.SubmittedDate! <= endOfCurrentMonthWeek3).Count();
            int current4thWeekReports = ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek4 && r!.SubmittedDate! <= endOfCurrentMonthWeek4).Count();

            bc!.CurrentMonthReport![0] = current1stWeekReports > 0 ? current1stWeekReports : 0;
            bc!.CurrentMonthReport![1] = current2ndWeekReports > 0 ? current2ndWeekReports : 0;
            bc!.CurrentMonthReport![2] = current3rdWeekReports > 0 ? current3rdWeekReports : 0;
            bc!.CurrentMonthReport![3] = current4thWeekReports > 0 ? current4thWeekReports : 0;
            //Get last week reports
            int last1stWeekReports = ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek1.AddMonths(-1) && r!.SubmittedDate! <= endOfCurrentMonthWeek1.AddMonths(-1)).Count();
            int last2ndWeekReports = ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek2.AddMonths(-1) && r!.SubmittedDate! <= endOfCurrentMonthWeek2.AddMonths(-1)).Count();
            int last3rdWeekReports = ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek3.AddMonths(-1) && r!.SubmittedDate! <= endOfCurrentMonthWeek3.AddMonths(-1)).Count();
            int last4thWeekReports = ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek4.AddMonths(-1) && r!.SubmittedDate! <= endOfLastMonthWeek4).Count();

            bc!.LastMonthReport![0] = last1stWeekReports > 0 ? last1stWeekReports : 0;
            bc!.LastMonthReport![1] = last2ndWeekReports > 0 ? last2ndWeekReports : 0;
            bc!.LastMonthReport![2] = last3rdWeekReports > 0 ? last3rdWeekReports : 0;
            bc!.LastMonthReport![3] = last4thWeekReports > 0 ? last4thWeekReports : 0;
            //1.5s 2nd time 0.4s
            report.BarChart = bc;

            //Get famous recipe for staticstic  0.1s       
            foreach (var recipe in db.Recipes.Include(r => r.Likes).Include(r => r.Comments).Include(r => r.User).OrderByDescending(r => r!.Likes!.Count()).OrderByDescending(r => r!.Comments!.Count()).Take(5))
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

        public async Task<StoreDashboardReport> generateStoreReport(Guid storeId)
        {
            StoreDashboardReport report = new StoreDashboardReport();
            var db = new CakeCuriousDbContext();
            StoreDashboardCardStats cs = new StoreDashboardCardStats();
            //Product Sold by month
            cs.CurrentMonthProductSold = await db.OrderDetails.Include(ord => ord.Order).Where(ord => ord!.Order!.OrderDate!.Value.Month == DateTime.Now.Month && ord!.Order!.Status! == (int)OrderStatusEnum.Completed && ord!.Order!.StoreId == storeId).GroupBy(ord => ord.ProductId).CountAsync();
            decimal LastMonthProductSold = await db.OrderDetails.Include(ord => ord.Order).Where(ord => ord!.Order!.OrderDate!.Value.Month == DateTime.Now.AddMonths(-1).Month && ord!.Order!.Status! == (int)OrderStatusEnum.Completed).GroupBy(ord => ord.ProductId).CountAsync();
            double sinceLastMonthNewUser = LastMonthProductSold > 0 || cs.CurrentMonthProductSold > 0 ? (double)((cs.CurrentMonthProductSold - LastMonthProductSold) / (cs.CurrentMonthProductSold > LastMonthProductSold ? cs.CurrentMonthProductSold : LastMonthProductSold)) : 0;
            cs.SinceLastMonthProductSold = Math.Round(sinceLastMonthNewUser, 2);
            //Sales by week
            DateTime startAtSunday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(DayOfWeek.Sunday - DateTime.Now.DayOfWeek).Day);
            foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startAtSunday && ord!.OrderDate! <= DateTime.Now && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    cs.CurrentWeekSales += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                cs.CurrentWeekSales -= ord!.DiscountedTotal!.Value > 0 ? ord!.DiscountedTotal!.Value : 0;
            }
            decimal lastWeekSales = 0;
            foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startAtSunday.AddDays(-7) && ord!.OrderDate! <= startAtSunday.AddDays(-1) && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    lastWeekSales += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                lastWeekSales -= ord!.DiscountedTotal!.Value > 0 ? ord!.DiscountedTotal!.Value : 0;
            }
            double sinceLastWeekSales = cs.CurrentWeekSales > 0 || lastWeekSales > 0 ? (double)((cs.CurrentWeekSales - lastWeekSales) / (cs.CurrentWeekSales > lastWeekSales ? cs.CurrentWeekSales : lastWeekSales)) : 0;
            cs.SinceLastWeekSales = Math.Round(sinceLastWeekSales, 2);
            //Order Complete by month
            cs.CurrentMonthTotalCompletedOrder = await db.Orders.Where(ord => ord!.OrderDate!.Value.Month == DateTime.Now.Month && ord!.Status! == (int)OrderStatusEnum.Completed && ord!.StoreId == storeId).CountAsync();
            decimal LastMonthCompletedOrder = await db.Orders.Where(ord => ord!.OrderDate!.Value.Month == DateTime.Now.AddMonths(-1).Month && ord!.Status! == (int)OrderStatusEnum.Completed && ord!.StoreId == storeId).CountAsync();
            double sinceLastMonthCompleteOrder = LastMonthCompletedOrder > 0 && cs.CurrentMonthTotalCompletedOrder > 0 ? (double)((cs.CurrentMonthTotalCompletedOrder - LastMonthCompletedOrder) / (cs.CurrentMonthTotalCompletedOrder > LastMonthCompletedOrder ? cs.CurrentMonthTotalCompletedOrder : LastMonthCompletedOrder)) : -1;
            cs.SinceLastMonthTotalCompletedOrder = Math.Round(sinceLastMonthCompleteOrder, 2);
            report.CardStats = cs;
            //Bar chart
            StoreDashboardBarChart bc = new StoreDashboardBarChart();
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;
            int lastMonth = DateTime.Now.AddMonths(-1).Month;
            int dateOfCurrentMonth = DateTime.DaysInMonth(currentYear, currentMonth);
            int dateOfLastMonth = DateTime.DaysInMonth(currentYear, lastMonth);
            //Get date for start,end of week in current month
            DateTime startOfCurrentMonthWeek1 = new DateTime(currentYear, currentMonth, 1);
            DateTime endOfCurrentMonthWeek1 = new DateTime(currentYear, currentMonth, 7);
            DateTime startOfCurrentMonthWeek2 = new DateTime(currentYear, currentMonth, 8);
            DateTime endOfCurrentMonthWeek2 = new DateTime(currentYear, currentMonth, 15);
            DateTime startOfCurrentMonthWeek3 = new DateTime(currentYear, currentMonth, 16);
            DateTime endOfCurrentMonthWeek3 = new DateTime(currentYear, currentMonth, 23);
            DateTime startOfCurrentMonthWeek4 = new DateTime(currentYear, currentMonth, 24);
            DateTime endOfCurrentMonthWeek4 = new DateTime(currentYear, currentMonth, dateOfCurrentMonth);
            DateTime endOfLastMonthWeek4 = new DateTime(currentYear, lastMonth, dateOfLastMonth);
            //init value for current week sale
            decimal currentWeekSale = 0;
            //get data for week1 sale of this month
            foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek1 && ord!.OrderDate! <= endOfCurrentMonthWeek1 && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
            }
            bc!.CurrentMonthSales![0] = currentWeekSale;
            //get data for week2 sale of this month
            currentWeekSale = 0;
            foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek2 && ord!.OrderDate! <= endOfCurrentMonthWeek2 && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
            }
            bc!.CurrentMonthSales![1] = currentWeekSale;
            //get data for week3 sale of this month
            currentWeekSale = 0;
            foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek3 && ord!.OrderDate! <= endOfCurrentMonthWeek3 && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
            }
            bc!.CurrentMonthSales![2] = currentWeekSale;
            //get data for week4 sale of this month
            currentWeekSale = 0;
            foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek4 && ord!.OrderDate! <= endOfCurrentMonthWeek4 && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
            }
            bc!.CurrentMonthSales![3] = currentWeekSale;
            //get data for week1 sale of last month
            currentWeekSale = 0;
            foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek1.AddMonths(-1) && ord!.OrderDate! <= endOfCurrentMonthWeek1.AddMonths(-1) && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
            }
            bc!.LastMonthSales![0] = currentWeekSale;
            //get data for week1 sale of last month
            currentWeekSale = 0;
            foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek2.AddMonths(-1) && ord!.OrderDate! <= endOfCurrentMonthWeek2.AddMonths(-1) && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
            }
            bc!.LastMonthSales![1] = currentWeekSale;
            //get data for week1 sale of last month
            currentWeekSale = 0;
            foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek3.AddMonths(-1) && ord!.OrderDate! <= endOfCurrentMonthWeek3.AddMonths(-1) && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
            }
            bc!.LastMonthSales![2] = currentWeekSale;
            //get data for week4 sale of last month
            currentWeekSale = 0;
            foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek4.AddMonths(-1) && ord!.OrderDate! <= endOfLastMonthWeek4 && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
            }
            bc!.LastMonthSales![3] = currentWeekSale;
            report.BarChart = bc;
            return report;
        }
    }
}
