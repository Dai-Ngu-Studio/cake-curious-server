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
using Repository.Models.Reports;
using Repository.Models.Users;
using System.Text;

namespace Repository
{
    public class ReportRepository : IViolationReportRepository
    {
        public async Task Add(ViolationReport obj)
        {
            var db = new CakeCuriousDbContext();
            await db.ViolationReports.AddAsync(obj);
            await db.SaveChangesAsync();
        }

        public IEnumerable<StaffDashboardReport> FilterByComment(IEnumerable<StaffDashboardReport> reports)
        {
            return reports.Where(p => p.ItemType == (int)ItemTypeEnum.Comment);
        }

        public IEnumerable<StaffDashboardReport> FilterByRecipe(IEnumerable<StaffDashboardReport> reports)
        {

            return reports.Where(p => p.ItemType == (int)ItemTypeEnum.Recipe);
        }

        public IEnumerable<StaffDashboardReport> OrderByAscTitle(IEnumerable<StaffDashboardReport> reports)
        {

            return reports.OrderBy(p => p.Title);
        }
        public IEnumerable<StaffDashboardReport> OrderByDescTitle(IEnumerable<StaffDashboardReport> reports)
        {
            return reports.OrderByDescending(p => p.Title);
        }

        public IEnumerable<StaffDashboardReport> FilterByPendingStatus(IEnumerable<StaffDashboardReport> reports)
        {
            return reports.Where(p => p.Status == (int)ReportStatusEnum.Pending);
        }
        public IEnumerable<StaffDashboardReport> FilterByRejectedStatus(IEnumerable<StaffDashboardReport> reports)
        {
            return reports.Where(p => p.Status == (int)ReportStatusEnum.Rejected);
        }
        public IEnumerable<StaffDashboardReport> FilterByCensoredStatus(IEnumerable<StaffDashboardReport> reports)
        {
            return reports.Where(p => p.Status == (int)ReportStatusEnum.Censored);
        }

        public IEnumerable<StaffDashboardReport> SearchViolationReport(string? keyWord, IEnumerable<StaffDashboardReport> reports)
        {
            return reports.Where(p => p.Title!.ToLower().Contains(keyWord!.ToLower()));
        }
        public async Task<SimpleUser?> getReportedUser(Guid? itemId, int? ItemType)
        {
            var db = new CakeCuriousDbContext();
            if (ItemType!.Value == (int)ItemTypeEnum.Comment)
            {
                Comment? comment = await db.Comments.Include(c => c.User).SingleOrDefaultAsync(c => c.Id == itemId);
                return comment!.User!.Adapt<SimpleUser>();
            }
            else if (ItemType!.Value == (int)ItemTypeEnum.Recipe)
            {
                Recipe? recipe = await db.Recipes.Include(r => r.User).SingleOrDefaultAsync(r => r.Id == itemId);
                return recipe!.User!.Adapt<SimpleUser>();
            }
            return null;
        }
        public async Task<StaffReportDetail?> GetReportDetailById(Guid id)
        {
            var db = new CakeCuriousDbContext();
            StaffReportDetail? report = await db.ViolationReports.Include(r => r.ReportCategory).Include(r => r.Staff).Include(r => r.Reporter).ProjectToType<StaffReportDetail>().FirstOrDefaultAsync(x => x.Id == id);
            if (report!.ItemType == (int)ItemTypeEnum.Recipe)
            {
                report.Recipe = await db.Recipes.Include(r => r.User).Include(r => r.HasCategories)!.ThenInclude(r => r.RecipeCategory).Include(r => r.RecipeMaterials).Include(r => r.RecipeMedia).Include(r => r.RecipeSteps).ProjectToType<SimpleRecipeForReportList>().FirstOrDefaultAsync(r => r.Id == report.ItemId);
            }
            else
            {
                report.Comment = await db.Comments.Include(r => r.User).Include(c => c.Images).ProjectToType<SimpleCommentForReportList>().FirstOrDefaultAsync(r => r.Id == report.ItemId);
            }
            return report;
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

        public async Task<ViolationReport?> GetById(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.ViolationReports.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<StaffDashboardReport>?> GetReportsOfAnItem(Guid itemId, string? s, string? order_by, string? filter_status, int PageIndex, int PageSize)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<StaffDashboardReport> reports = db.ViolationReports.Where(r => r.ItemId == itemId).Include(r => r.ReportCategory).Include(r => r.Staff).Include(r => r.Reporter).ProjectToType<StaffDashboardReport>();
            foreach (var report in reports)
            {
                if (report.ItemType.HasValue && report.ItemId.HasValue)
                    report.ReportedUser = await getReportedUser(report.ItemId, report.ItemType)!;
            }
            try
            {
                //search
                if (s != null)
                {
                    reports = SearchViolationReport(s, reports);
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
                return reports.Skip((PageIndex - 1) * PageSize)
                            .Take(PageSize).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public int CountDashboardViolationReportsOfAnItem(Guid itemId, string? s, string? order_by, string? filter)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<StaffDashboardReport> reports = db.ViolationReports.Where(r => r.ItemId == itemId).Include(r => r.ReportCategory).Include(r => r.Staff).Include(r => r.Reporter).ProjectToType<StaffDashboardReport>();
            try
            {
                //search
                if (s != null)
                {
                    reports = SearchViolationReport(s, reports);
                }
                //filter status
                if (filter != null && filter == ReportStatusEnum.Censored.ToString())
                {
                    reports = FilterByCensoredStatus(reports);
                }
                else if (filter != null && filter == ReportStatusEnum.Pending.ToString())
                {
                    reports = FilterByPendingStatus(reports);
                }
                else if (filter != null && filter == ReportStatusEnum.Rejected.ToString())
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
                return reports.Count();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return 0;
        }

        public async Task<int> CountPendingReportOfAnItem(Guid itemId)
        {
            var db = new CakeCuriousDbContext();
            return await db.ViolationReports.Where(r => r.ItemId == itemId && r.Status == (int)ReportStatusEnum.Pending).CountAsync();
        }

        public async Task<string?> BulkUpdate(Guid[] ids, string uid)
        {
            var db = new CakeCuriousDbContext();
            if (ids.Count() > 0)
            {
                var stringBuilder = new StringBuilder();
                foreach (Guid id in ids)
                {
                    ViolationReport? report = await db.ViolationReports.FirstOrDefaultAsync(r => r.Id == id);
                    if (report!.Status != null
                   &&
                   (report.Status == (int)ReportStatusEnum.Rejected
                   || report.Status == (int)ReportStatusEnum.Censored)) stringBuilder.AppendLine(" " + report.Id.ToString());
                    else
                    {
                        report!.Status = (int)ReportStatusEnum.Rejected;
                        report!.StaffId = uid;
                    }
                    db.Entry<ViolationReport>(report).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
                if (stringBuilder.Length != 0) return stringBuilder.ToString();
            }
            return null;
        }

        public async Task UpdateAllReportStatusOfAnItem(Guid itemId, string uid)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<ViolationReport> reports = db.ViolationReports.Where(r => r.ItemId == itemId);
            foreach (ViolationReport report in reports)
            {
                if (report.Status == (int)ReportStatusEnum.Pending)
                {
                    report.Status = (int)ReportStatusEnum.Censored;
                    report.StaffId = uid;
                }
            }
            db.UpdateRange(reports);
            await db.SaveChangesAsync();
        }
    }
}
