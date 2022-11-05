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
            cs.CurrentMonthNewBaker = await db.Users.Where(u => u!.CreatedDate!.Value.Month == DateTime.Now.Month).CountAsync();
            decimal LastMonthNewUser = await db.Users.Where(u => u!.CreatedDate!.Value.Month == DateTime.Now.AddMonths(-1).Month).CountAsync();
            double sinceLastMonthNewUser = LastMonthNewUser > 0 && cs.CurrentMonthNewBaker > 0 ? (double)((cs.CurrentMonthNewBaker - LastMonthNewUser) / (cs.CurrentMonthNewBaker > LastMonthNewUser ? cs.CurrentMonthNewBaker : LastMonthNewUser)) : -1;
            cs.SinceLastMonthNewBaker = sinceLastMonthNewUser;

            //new store by month
            cs.CurrentMonthNewStore = await db.Stores.Where(s => s!.CreatedDate!.Value.Month == DateTime.Now.Month).CountAsync();
            decimal LastMonthNewStore = await db.Stores.Where(s => s!.CreatedDate!.Value.Month == DateTime.Now.AddMonths(-1).Month).CountAsync();
            double sinceLastMonthNewStore = cs.CurrentMonthNewStore > 0 && LastMonthNewStore > 0 ? (double)((cs.CurrentMonthNewStore - LastMonthNewStore) / (cs.CurrentMonthNewStore > LastMonthNewStore ? cs.CurrentMonthNewStore : LastMonthNewStore)) : -1;
            cs.SinceLastMonthNewStore = sinceLastMonthNewStore;
            report.CardStats = cs;

            //Add barchart
            AdminDashboardBarChart bc = new AdminDashboardBarChart();
            int Week = 1;
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

            while (Week < 5)
            {
                if (Week == 1)
                {
                    bc!.CurrentWeekReport!.Add(await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek1 && r!.SubmittedDate! <= endOfCurrentMonthWeek1).CountAsync() > 0 ? await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek1 && r!.SubmittedDate! <= endOfCurrentMonthWeek1).CountAsync() : 0);
                }
                if (Week == 2)
                {
                    bc!.CurrentWeekReport!.Add(await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek2 && r!.SubmittedDate! <= endOfCurrentMonthWeek2).CountAsync() > 0 ? await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek2 && r!.SubmittedDate! <= endOfCurrentMonthWeek2).CountAsync() : 0);
                }
                if (Week == 3)
                {
                    bc!.CurrentWeekReport!.Add(await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek3 && r!.SubmittedDate! <= endOfCurrentMonthWeek3).CountAsync() > 0 ? await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek3 && r!.SubmittedDate! <= endOfCurrentMonthWeek3).CountAsync() : 0);
                }
                if (Week == 4)
                {
                    bc!.CurrentWeekReport!.Add(await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek4 && r!.SubmittedDate! <= endOfCurrentMonthWeek4).CountAsync() > 0 ? await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek4 && r!.SubmittedDate! <= endOfCurrentMonthWeek4).CountAsync() : 0);
                }
                Week++;
            }
            Week = 1;
            while (Week < 5)
            {
                if (Week == 1)
                {
                    bc!.LastWeekReport!.Add(await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek1.AddMonths(-1) && r!.SubmittedDate! <= endOfCurrentMonthWeek1.AddMonths(-1)).CountAsync() > 0 ? await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek1.AddMonths(-1) && r!.SubmittedDate! <= endOfCurrentMonthWeek1.AddMonths(-1)).CountAsync() : 0);
                }
                if (Week == 2)
                {
                    bc!.LastWeekReport!.Add(await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek2.AddMonths(-1) && r!.SubmittedDate! <= endOfCurrentMonthWeek2.AddMonths(-1)).CountAsync() > 0 ? await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek2.AddMonths(-1) && r!.SubmittedDate! <= endOfCurrentMonthWeek2.AddMonths(-1)).CountAsync() : 0);
                }
                if (Week == 3)
                {
                    bc!.LastWeekReport!.Add(await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek3.AddMonths(-1) && r!.SubmittedDate! <= endOfCurrentMonthWeek3.AddMonths(-1)).CountAsync() > 0 ? await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek3.AddMonths(-1) && r!.SubmittedDate! <= endOfCurrentMonthWeek3.AddMonths(-1)).CountAsync() : 0);
                }
                if (Week == 4)
                {
                    bc!.LastWeekReport!.Add(await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek4.AddMonths(-1) && r!.SubmittedDate! <= endOfLastMonthWeek4).CountAsync() > 0 ? await db.ViolationReports.Where(r => r!.SubmittedDate! >= startOfCurrentMonthWeek4.AddMonths(-1) && r!.SubmittedDate! <= endOfCurrentMonthWeek4.AddMonths(-1)).CountAsync() : 0);
                }
                Week++;
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

        public async Task<StoreDashboardReport> generateStoreReport(Guid storeId)
        {
            StoreDashboardReport report = new StoreDashboardReport();
            var db = new CakeCuriousDbContext();
            StoreDashboardCardStats cs = new StoreDashboardCardStats();

            //Product Sold by month
            cs.CurrentMonthProductSold = await db.OrderDetails.Include(ord => ord.Order).Where(ord => ord!.Order!.OrderDate!.Value.Month == DateTime.Now.Month &&  ord!.Order!.Status! == (int)OrderStatusEnum.Completed && ord!.Order!.StoreId == storeId).GroupBy(ord => ord.ProductId).CountAsync();
            decimal LastMonthProductSold = await db.OrderDetails.Include(ord => ord.Order).Where(ord => ord!.Order!.OrderDate!.Value.Month == DateTime.Now.AddMonths(-1).Month && ord!.Order!.Status! == (int)OrderStatusEnum.Completed).GroupBy(ord => ord.ProductId).CountAsync();
            double sinceLastMonthNewUser = LastMonthProductSold > 0 && cs.CurrentMonthProductSold > 0 ? (double)((cs.CurrentMonthProductSold - LastMonthProductSold) / (cs.CurrentMonthProductSold > LastMonthProductSold ? cs.CurrentMonthProductSold : LastMonthProductSold)) : -1;
            cs.SinceLastMonthProductSold = sinceLastMonthNewUser;
            //Sales by week
            DateTime startAtMonday = DateTime.Now.AddDays(DayOfWeek.Monday - DateTime.Now.DayOfWeek);
            foreach (var ord in  db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startAtMonday &&  ord!.OrderDate! <= DateTime.Now && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    cs.CurrentWeekSales += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                cs.CurrentWeekSales -= ord!.DiscountedTotal!.Value > 0 ? ord!.DiscountedTotal!.Value : 0;
            }
            decimal lastWeekSales = 0;
            foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startAtMonday.AddDays(-7) && ord!.OrderDate! <= startAtMonday.AddDays(-1) && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
            {
                foreach (var orderDetail in ord!.OrderDetails!)
                {
                    lastWeekSales += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                }
                lastWeekSales -= ord!.DiscountedTotal!.Value > 0 ? ord!.DiscountedTotal!.Value : 0;
            }
            double sinceLastWeekSales = cs.CurrentWeekSales > 0 && lastWeekSales > 0 ? (double)((cs.CurrentWeekSales - lastWeekSales) / (cs.CurrentWeekSales > lastWeekSales ? cs.CurrentWeekSales : lastWeekSales)) : -1;
            cs.SinceLastWeekSales = sinceLastWeekSales;
            //Order Complete by month
            cs.CurrentMonthTotalCompletedOrder = await db.Orders.Where(ord => ord!.OrderDate!.Value.Month == DateTime.Now.Month && ord!.Status! == (int)OrderStatusEnum.Completed && ord!.StoreId == storeId).CountAsync();
            decimal LastMonthCompletedOrder = await db.Orders.Where(ord => ord!.OrderDate!.Value.Month == DateTime.Now.AddMonths(-1).Month && ord!.Status! == (int)OrderStatusEnum.Completed && ord!.StoreId == storeId).CountAsync();
            double sinceLastMonthCompleteOrder = LastMonthCompletedOrder > 0 && cs.CurrentMonthTotalCompletedOrder > 0 ? (double)((cs.CurrentMonthTotalCompletedOrder - LastMonthCompletedOrder) / (cs.CurrentMonthTotalCompletedOrder > LastMonthCompletedOrder ? cs.CurrentMonthTotalCompletedOrder : LastMonthCompletedOrder)) : -1;
            cs.SinceLastMonthTotalCompletedOrder = sinceLastMonthNewUser;
            report.CardStats = cs;
            //Bar chart
            StoreDashboardBarChart bc = new StoreDashboardBarChart();
            int Week = 1;
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

            while (Week < 5)
            {
                if (Week == 1)
                {
                    decimal currentWeekSale = 0;
                    foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek1 && ord!.OrderDate! <= endOfCurrentMonthWeek1 && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
                    {
                        foreach (var orderDetail in ord!.OrderDetails!)
                        {
                            currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                        }
                        currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
                    }
                    bc!.CurrentWeekSales!.Add(currentWeekSale);
                }
                if (Week == 2)
                {
                    decimal currentWeekSale = 0;
                    foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek2 && ord!.OrderDate! <= endOfCurrentMonthWeek2 && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
                    {
                        foreach (var orderDetail in ord!.OrderDetails!)
                        {
                            currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                        }
                        currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
                    }
                    bc!.CurrentWeekSales!.Add(currentWeekSale);
                }
                if (Week == 3)
                {
                    decimal currentWeekSale = 0;
                    foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek3 && ord!.OrderDate! <= endOfCurrentMonthWeek3 && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
                    {
                        foreach (var orderDetail in ord!.OrderDetails!)
                        {
                            currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                        }
                        currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
                    }
                    bc!.CurrentWeekSales!.Add(currentWeekSale);
                }
                if (Week == 4)
                {
                    decimal currentWeekSale = 0;
                    foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek4 && ord!.OrderDate! <= endOfCurrentMonthWeek4 && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
                    {
                        foreach (var orderDetail in ord!.OrderDetails!)
                        {
                            currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                        }
                        currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
                    }
                    bc!.CurrentWeekSales!.Add(currentWeekSale);
                }
                Week++;
            }
            Week = 1;
            while (Week < 5)
            {
                if (Week == 1)
                {
                    decimal currentWeekSale = 0;
                    foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek1.AddMonths(-1) && ord!.OrderDate! <= endOfCurrentMonthWeek1.AddMonths(-1) && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
                    {
                        foreach (var orderDetail in ord!.OrderDetails!)
                        {
                            currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                        }
                        currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
                    }
                    bc!.LastWeekSales!.Add(currentWeekSale);
                }
                if (Week == 2)
                {
                    decimal currentWeekSale = 0;
                    foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek2.AddMonths(-1) && ord!.OrderDate! <= endOfCurrentMonthWeek2.AddMonths(-1) && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
                    {
                        foreach (var orderDetail in ord!.OrderDetails!)
                        {
                            currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                        }
                        currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
                    }
                    bc!.LastWeekSales!.Add(currentWeekSale);
                }
                if (Week == 3)
                {
                    decimal currentWeekSale = 0;
                    foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek3.AddMonths(-1) && ord!.OrderDate! <= endOfCurrentMonthWeek3.AddMonths(-1) && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
                    {
                        foreach (var orderDetail in ord!.OrderDetails!)
                        {
                            currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                        }
                        currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
                    }
                    bc!.LastWeekSales!.Add(currentWeekSale);
                }
                if (Week == 4)
                {
                    decimal currentWeekSale = 0;
                    foreach (var ord in db.Orders.Include(ord => ord.OrderDetails).Where(ord => ord!.OrderDate! >= startOfCurrentMonthWeek4.AddMonths(-1) && ord!.OrderDate! <= endOfLastMonthWeek4 && ord!.StoreId == storeId && ord!.Status! == (int)OrderStatusEnum.Completed))
                    {
                        foreach (var orderDetail in ord!.OrderDetails!)
                        {
                            currentWeekSale += orderDetail!.Quantity!.Value * orderDetail!.Price!.Value;
                        }
                        currentWeekSale -= ord!.DiscountedTotal != null ? ord!.DiscountedTotal!.Value : 0;
                    }
                    bc!.LastWeekSales!.Add(currentWeekSale);
                }
                Week++;
            }
            report.BarChart = bc;
            return report;
        }
    }
}
