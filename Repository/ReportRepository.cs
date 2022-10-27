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
using Repository.Models.RecipeMedia;
using Repository.Models.Recipes;
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
        public IEnumerable<StaffDashboardReport> FilterByRejectedStatus(IEnumerable<StaffDashboardReport> reports)
        {
            return reports.Where(p => p.Status == (int)ReportStatusEnum.Rejected).ToList();
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
        public async Task<IEnumerable<StaffDashboardReport>?> GetViolationReports(string? s, string? order_by, string? filter_type, string? filter_status, int pageIndex, int pageSize)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<StaffDashboardReport> reports = await db.ViolationReports.Include(r => r.ReportCategory).Include(r => r.Staff).Include(r => r.Reporter).ProjectToType<StaffDashboardReport>().ToListAsync();
            foreach (var report in reports)
            {
                report.ReportedUser = await getReportedUser(report.ItemId);
            }
            try
            {
                //search
                if (s != null)
                {
                    reports = SearchViolationReport(s, reports);
                }
                //filter type
                if (filter_type != null && filter_type == ReportTypeEnum.Comment.ToString())
                {
                    reports = FilterByComment(reports);
                }
                else if (filter_type != null && filter_type == ReportTypeEnum.Recipe.ToString())
                {
                    reports = FilterByRecipe(reports);
                }
                //filter status
                if (filter_status != null && filter_status == ReportStatusEnum.Censored.ToString())
                {
                    reports = FilterByCensoredStatus(reports);
                }
                else if (filter_status != null && filter_status == ReportStatusEnum.Pending.ToString())
                {
                    reports = FilterByPendingStatus(reports);
                }
                else if (filter_status != null && filter_status == ReportStatusEnum.Rejected.ToString())
                {
                    reports = FilterByRejectedStatus(reports);
                }
                //sort
                if (order_by != null && order_by == ReportSortEnum.DescTitle.ToString())
                {
                    reports = OrderByDescTitle(reports);
                }
                else if (order_by != null && order_by == ReportSortEnum.AscTitle.ToString())
                {
                    reports = OrderByAscTitle(reports);
                }

                reports = reports.Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize).ToList();
                foreach (var report in reports)
                {
                    if (report.ItemType == (int)ItemTypeEnum.Recipe)
                    {
                        report.Recipe = await db.Recipes.Include(r => r.User).Include(r => r.HasCategories)!.ThenInclude(r => r.RecipeCategory).Include(r => r.RecipeMaterials).Include(r => r.RecipeMedia).Include(r => r.RecipeSteps).ProjectToType<SimpleRecipeForReportList>().FirstOrDefaultAsync(r => r.Id == report.ItemId);
                    }
                    else
                    {
                        report.Comment = await db.Comments.Include(r => r.User).Include(c => c.Images).ProjectToType<SimpleCommentForReportList>().FirstOrDefaultAsync(r => r.Id == report.ItemId);
                    }
                }
                return reports;
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
            return await db.ViolationReports.Include(r => r.Staff).FirstOrDefaultAsync(x => x.Id == id);
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

        public int CountDashboardViolationReports(string? s, string? order_by, string? filter_type, string? filter_status)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<StaffDashboardReport> reports = db.ViolationReports.Include(r => r.Reporter).Adapt<IEnumerable<StaffDashboardReport>>().ToList();
            try
            {
                if (s != null)
                {
                    reports = SearchViolationReport(s, reports);
                }
                //filter type
                if (filter_type != null && filter_type == ReportTypeEnum.Comment.ToString())
                {
                    reports = FilterByComment(reports);
                }
                else if (filter_type != null && filter_type == ReportTypeEnum.Recipe.ToString())
                {
                    reports = FilterByRecipe(reports);
                }
                //filter status
                if (filter_status != null && filter_status == ReportStatusEnum.Censored.ToString())
                {
                    reports = FilterByCensoredStatus(reports);
                }
                else if (filter_status != null && filter_status == ReportStatusEnum.Pending.ToString())
                {
                    reports = FilterByPendingStatus(reports);
                }
                if (order_by != null && order_by == ReportSortEnum.DescTitle.ToString())
                {
                    reports = OrderByDescTitle(reports);
                }
                else if (order_by != null && order_by == ReportSortEnum.AscTitle.ToString())
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
