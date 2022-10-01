using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Products;
using Repository.Constants.RecipeMaterials;
using Repository.Constants.Reports;
using Repository.Interfaces;
using Repository.Models;
using Repository.Models.Comments;
using Repository.Models.Product;
using Repository.Models.RecipeMaterials;
using Repository.Models.RecipeMedias;
using Repository.Models.RecipeSteps;
using Repository.Models.Users;

namespace Repository
{
    public class ReportRepository : IViolationReportRepository
    {
        public async Task Add(ViolationReport obj)
        {
            var db = new CakeCuriousDbContext();
            db.ViolationReports.Add(obj);
            await db.SaveChangesAsync();
        }

        public IEnumerable<StaffDashboardReport> FilterByComment(IEnumerable<StaffDashboardReport> reports)
        {
            return reports.Where(p => p.ItemType == (int)ItemTypeEnum.Comment).ToList();
        }

        public IEnumerable<StaffDashboardReport> FilterByRecipe(IEnumerable<StaffDashboardReport> reports)
        {

            return reports.Where(p => p.ItemType == (int)ItemTypeEnum.Recipe).ToList();
        }

        public IEnumerable<StaffDashboardReport> OrderByAscTitle(IEnumerable<StaffDashboardReport> reports)
        {

            return reports.OrderBy(p => p.Title).ToList();
        }
        public IEnumerable<StaffDashboardReport> OrderByDescTitle(IEnumerable<StaffDashboardReport> reports)
        {
            return reports.OrderByDescending(p => p.Title).ToList();
        }

        public IEnumerable<StaffDashboardReport> FilterByPendingStatus(IEnumerable<StaffDashboardReport> reports)
        {
            return reports.Where(p => p.Status == (int)ReportStatusEnum.Pending).ToList();
        }
        public IEnumerable<StaffDashboardReport> FilterByCensoredStatus(IEnumerable<StaffDashboardReport> reports)
        {
            return reports.Where(p => p.Status == (int)ReportStatusEnum.Censored).ToList();
        }

        public IEnumerable<StaffDashboardReport> SearchViolationReport(string? keyWord, IEnumerable<StaffDashboardReport> reports)
        {
            return reports.Where(p => p.Title!.Contains(keyWord!)).ToList();
        }
        public async Task<SimpleUser?> getReportedUser(Guid? itemId)
        {
            var db = new CakeCuriousDbContext();
            Recipe? recipe = await db.Recipes.Include(r => r.User).SingleOrDefaultAsync(r => r.Id == itemId);
            if (recipe == null)
            {
                Comment? comment = await db.Comments.Include(c => c.User).SingleOrDefaultAsync(c => c.Id == itemId);
                return comment!.User!.Adapt<SimpleUser>();
            }
            else if (recipe != null)
            {
                return recipe!.User!.Adapt<SimpleUser>();
            }
            return null;
        }
        public async Task<IEnumerable<StaffDashboardReport>?> GetViolationReports(string? s, string? order_by, string? report_type, int pageIndex, int pageSize)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<StaffDashboardReport> reports = db.ViolationReports.Include(r => r.Reporter).Adapt<IEnumerable<StaffDashboardReport>>().ToList();
            foreach (var report in reports)
            {
                report.ReportedUser = await getReportedUser(report.ItemId);
                report.Staff = await db.Users.ProjectToType<SimpleUser>().SingleOrDefaultAsync(u => u.Id == report.StaffId);
            }
            try
            {
                if (s != null)
                {
                    reports = SearchViolationReport(s, reports);
                }
                if (report_type != null && report_type == ReportTypeEnum.Comment.ToString())
                {
                    reports = FilterByComment(reports);
                }
                else if (report_type != null && report_type == ReportTypeEnum.Recipe.ToString())
                {
                    reports = FilterByRecipe(reports);

                }
                if (order_by != null && order_by == ReportSortEnum.DescTitle.ToString())
                {
                    reports = OrderByDescTitle(reports);
                }
                else if (order_by != null && order_by == ReportSortEnum.AscTitle.ToString())
                {
                    reports = OrderByAscTitle(reports);
                }
                return reports.Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task<ViolationReport?> GetById(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.ViolationReports.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task Update(ViolationReport updateObj)
        {
            try
            {
                var db = new CakeCuriousDbContext();
                db.Entry<ViolationReport>(updateObj).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public int CountDashboardViolationReports(string? s, string? order_by, string? report_type)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<StaffDashboardReport> reports = db.ViolationReports.Include(r => r.Reporter).Adapt<IEnumerable<StaffDashboardReport>>().ToList();
            try
            {
                if (s != null)
                {
                    reports = SearchViolationReport(s, reports);
                }
                if (report_type != null && report_type == "ByComment")
                {
                    reports = FilterByComment(reports);
                }
                else if (report_type != null && report_type == "ByRecipe")
                {
                    reports = FilterByRecipe(reports);

                }
                if (order_by != null && order_by == "DescTitle")
                {
                    reports = OrderByDescTitle(reports);
                }
                else if (order_by != null && order_by == "AscTitle")
                {
                    reports = OrderByAscTitle(reports);
                }
                return reports.Count();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            return 0;
        }

        public async Task<ItemReportContent?> GetReportedItemDetail(Guid? itemId)
        {
            var db = new CakeCuriousDbContext();
            Recipe? recipe = await db.Recipes.Include(r => r.RecipeMedia).Include(r => r.RecipeSteps).Include(r => r.RecipeMaterials).SingleOrDefaultAsync(r => r.Id == itemId);
            if (recipe == null)
            {
                Comment? comment = await db.Comments.Include(c => c.Images).SingleOrDefaultAsync(c => c.Id == itemId);
                return new ItemReportContent
                {
                    commentContent = comment!.Content,
                    Id = itemId!.Value,
                    commentMedia = comment!.Images!.Adapt<ICollection<RecipeCommentMedia>>().ToList(),
                    ItemType = ItemTypeEnum.Comment.ToString(),
                };
            }
            else if (recipe != null)
            {
                return new ItemReportContent
                {
                    Id = itemId!.Value,
                    Desciption = recipe.Description,
                    Steps = recipe!.RecipeSteps!.Adapt<ICollection<SimpleRecipeStep>>().OrderBy(s => s.StepNumber).ToList(),
                    ImageLinks = recipe!.RecipeMedia!.Adapt<ICollection<SimpleRecipeMedia>>().Where(r => r!.MediaUrl!.Contains(".jpg") || r!.MediaUrl!.Contains(".png")).ToList(),
                    VideoLinks = recipe!.RecipeMedia!.Adapt<ICollection<SimpleRecipeMedia>>().Where(r => r!.MediaUrl!.Contains(".mp4")).ToList(),
                    RecipeMerterials = recipe!.RecipeMaterials!.Where(rm => rm.MaterialType == (int)RecipeMaterialTypeEnum.Ingredient).Adapt<ICollection<SimpleRecipeMaterial>>().ToList(),
                    ItemType = ItemTypeEnum.Recipe.ToString(),
                };
            }
            return null;
        }
    }
}
