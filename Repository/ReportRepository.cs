using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
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

        public IEnumerable<StaffDashboardReport> FilterByAscTitle(IEnumerable<StaffDashboardReport> reports)
        {

            return reports.OrderBy(p => p.Title).ToList();
        }
        public IEnumerable<StaffDashboardReport> FilterByDescTitle(IEnumerable<StaffDashboardReport> reports)
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

        public IEnumerable<StaffDashboardReport> SearchViolationReport(string? keyWord)
        {
            IEnumerable<StaffDashboardReport> reports;
            var db = new CakeCuriousDbContext();

            reports = keyWord != null 
                ? db.ViolationReports.Where(p => p.Title!.Contains(keyWord!)).Include(p => p.Reporter).ProjectToType<StaffDashboardReport>().ToList()
                : db.ViolationReports.Include(p => p.Reporter).ProjectToType<StaffDashboardReport>().ToList();
            return reports;
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
        public async Task<IEnumerable<StaffDashboardReport>?> GetViolationReports(string? s, string? filter_ViolationReport, int pageIndex, int pageSize)
        {
            IEnumerable<StaffDashboardReport> result;
            IEnumerable<StaffDashboardReport> reports = SearchViolationReport(s!);
            var db = new CakeCuriousDbContext();
            foreach (var report in reports)
            {
                report.ReportedUser = await getReportedUser(report.ItemId);
                report.Staff = await db.Users.ProjectToType<SimpleUser>().SingleOrDefaultAsync(u => u.Id == report.StaffId);
            }
            try
            {
                if (filter_ViolationReport == null)
                    return reports.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                else if (filter_ViolationReport == "ByComment")
                {
                    result = FilterByComment(reports);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
                else if (filter_ViolationReport == "ByRecipe")
                {
                    result = FilterByRecipe(reports);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
                else if (filter_ViolationReport == "ByDescendingTitle")
                {
                    result = FilterByDescTitle(reports);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
                else if (filter_ViolationReport == "ByAscendingTitle")
                {
                    result = FilterByAscTitle(reports);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
                else if (filter_ViolationReport == "ByCensoredStatus")
                {
                    result = FilterByCensoredStatus(reports);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
                else if (filter_ViolationReport == "ByPendingStatus")
                {
                    result = FilterByPendingStatus(reports);
                    return result.Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize).ToList();
                }
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

        public int CountDashboardViolationReports(string? s, string? filter_ViolationReport)
        {
            IEnumerable<StaffDashboardReport> result;
            IEnumerable<StaffDashboardReport> report = SearchViolationReport(s!);
            try
            {
                if (filter_ViolationReport == null)
                    return report.Count();
                else if (filter_ViolationReport == "ByComment")
                {
                    result = FilterByComment(report);
                    return result.Count();
                }
                else if (filter_ViolationReport == "ByRecipe")
                {
                    result = FilterByRecipe(report);
                    return result.Count();
                }
                else if (filter_ViolationReport == "ByDescendingTitle")
                {
                    result = FilterByDescTitle(report);
                    return result.Count();
                }
                else if (filter_ViolationReport == "ByAscendingTitle")
                {
                    result = FilterByAscTitle(report);
                    return result.Count();
                }
                else if (filter_ViolationReport == "ByInactiveStatus")
                {
                    result = FilterByPendingStatus(report);
                    return result.Count();
                }
                else if (filter_ViolationReport == "ByActiveStatus")
                {
                    result = FilterByCensoredStatus(report);
                    return result.Count();
                }
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
