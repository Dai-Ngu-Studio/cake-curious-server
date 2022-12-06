using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Comments;
using Repository.Constants.Reports;
using Repository.Constants.Users;
using Repository.Interfaces;
using Repository.Models.Comments;

namespace Repository
{
    public class CommentRepository : ICommentRepository
    {
        public async Task Add(Comment comment)
        {
            var db = new CakeCuriousDbContext();
            await db.Comments.AddAsync(comment);
            await db.SaveChangesAsync();
        }

        public async Task<Comment?> GetCommentReadonly(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Comments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Comment?> GetCommentWithRootReadonly(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Comments.AsNoTracking().Include(x => x.Root).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<RecipeComment?> GetRecipeComment(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Comments.AsNoTracking().Where(x => x.Id == id).ProjectToType<RecipeComment>().FirstOrDefaultAsync();
        }

        public async Task<int> Update(Guid id, UpdateComment updateComment)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                string query = "update [Comment] set [Comment].[content] = {0} where [Comment].[id] = {1}";
                var rows = await db.Database.ExecuteSqlRawAsync(query, updateComment.Content ?? "...", id);
                await transaction.CommitAsync();
                return rows;
            }
        }

        public async Task<int> Delete(Guid id)
        {
            var db = new CakeCuriousDbContext();
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                string query = "update [Comment] set [Comment].[status] = {0} where [Comment].[id] = {1} or [Comment].[root_id] = {1}";
                var rows = await db.Database.ExecuteSqlRawAsync(query, (int)CommentStatusEnum.Inactive, id);
                await transaction.CommitAsync();
                return rows;
            }
        }

        public async Task<int> CountRepliesForComment(Guid commentId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Comments
                .AsNoTracking()
                .Where(x => x.RootId == commentId)
                .Where(x => x.Status == (int)CommentStatusEnum.Active)
                .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                .CountAsync();
        }

        public IEnumerable<RecipeComment> GetRepliesForComment(Guid commentId, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Comments
                .AsNoTracking()
                .OrderBy(x => x.SubmittedDate)
                .Where(x => x.RootId == commentId)
                .Where(x => x.Status == (int)CommentStatusEnum.Active)
                .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                .Skip(skip)
                .Take(take)
                .ProjectToType<RecipeComment>();
        }

        public async Task<int> CountCommentsForRecipe(Guid recipeId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Comments
                .AsNoTracking()
                .Where(x => x.RecipeId == recipeId)
                .Where(x => x.RootId == null)
                .Where(x => x.Status == (int)CommentStatusEnum.Active)
                .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                .CountAsync();
        }

        public IEnumerable<RecipeComment> GetCommentsForRecipe(Guid recipeId, int skip, int take)
        {
            var db = new CakeCuriousDbContext();
            return db.Comments
                .AsNoTracking()
                .OrderByDescending(x => x.SubmittedDate)
                .Where(x => x.RecipeId == recipeId)
                .Where(x => x.RootId == null)
                .Where(x => x.Status == (int)CommentStatusEnum.Active)
                .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                .Skip(skip)
                .Take(take)
                .ProjectToType<RecipeComment>();
        }
        public List<SimpleCommentForReportList>? FilterByStatusActive(List<SimpleCommentForReportList>? isReportedComments)
        {
            return isReportedComments!.Where(p => p.Status == (int)CommentStatusEnum.Active).ToList();
        }

        public List<SimpleCommentForReportList>? FilterByStatusInactive(List<SimpleCommentForReportList>? isReportedComments)
        {
            return isReportedComments!.Where(p => p.Status == (int)CommentStatusEnum.Inactive).ToList();
        }
        public List<SimpleCommentForReportList>? OrderByAscTotalPendingReport(List<SimpleCommentForReportList>? isReportedComments)
        {
            return isReportedComments!.OrderBy(r => r.TotalPendingReports).ToList();
        }
        public List<SimpleCommentForReportList>? OrderByDescTotalPendingReport(List<SimpleCommentForReportList>? isReportedComments)
        {
            return isReportedComments!.OrderByDescending(r => r.TotalPendingReports).ToList();
        }
        public async Task<IEnumerable<SimpleCommentForReportList>> GetReportedCommments(string? filter, string? sort, int page, int size)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<ViolationReport> reportsCommentType = await db.ViolationReports.Where(report => report.ItemType == (int)ReportTypeEnum.Comment).GroupBy(x => x.ItemId).Select(d => d.First()).ToListAsync();
            List<SimpleCommentForReportList>? isReportedComments = new List<SimpleCommentForReportList>();
            if (reportsCommentType.Count() > 0)
                foreach (var report in reportsCommentType)
                {
                    SimpleCommentForReportList comment = (await db.Comments.ProjectToType<SimpleCommentForReportList>().FirstOrDefaultAsync(r => r.Id == report!.ItemId))!;
                    comment.TotalPendingReports = await db.ViolationReports.Where(report => report.Status == (int)ReportStatusEnum.Pending && report.ItemId == comment.Id).CountAsync();
                    isReportedComments!.Add(comment);
                }
            //filter
            if (filter != null && filter == CommentStatusEnum.Active.ToString())
            {
                isReportedComments = FilterByStatusActive(isReportedComments);
            }
            else if (filter != null && filter == CommentStatusEnum.Inactive.ToString())
            {
                isReportedComments = FilterByStatusInactive(isReportedComments);
            }
            //order by
            if (sort != null && sort == SortCommentEnum.AscPendingReport.ToString())
            {
                isReportedComments = OrderByAscTotalPendingReport(isReportedComments);
            }
            else if (sort != null && sort == SortCommentEnum.DescPendingReport.ToString())
            {
                isReportedComments = OrderByDescTotalPendingReport(isReportedComments);
            }
            return isReportedComments!.Skip((page - 1) * size)
                                .Take(size).OrderByDescending(c => c.SubmittedDate).ToList();
        }

        public async Task<int?> CountReportedCommmentsTotalPage(string? filter)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<ViolationReport> reportsRecipeType = await db.ViolationReports.Where(report => report.ItemType == (int)ReportTypeEnum.Comment).GroupBy(x => x.ItemId).Select(d => d.First()).ToListAsync();
            List<SimpleCommentForReportList>? isReportedComments = new List<SimpleCommentForReportList>();
            if (reportsRecipeType.Count() > 0)
                foreach (var report in reportsRecipeType)
                {
                    isReportedComments!.Add((await db.Comments.ProjectToType<SimpleCommentForReportList>().FirstOrDefaultAsync(r => r.Id == report!.ItemId))!);
                }
            //filter
            if (filter != null && filter == CommentStatusEnum.Active.ToString())
            {
                isReportedComments = FilterByStatusActive(isReportedComments);
            }
            else if (filter != null && filter == CommentStatusEnum.Inactive.ToString())
            {
                isReportedComments = FilterByStatusInactive(isReportedComments);
            }
            return isReportedComments!.Count();
        }

        public async Task<SimpleComment> GetCommentById(Guid id)
        {
            var db = new CakeCuriousDbContext();
            SimpleComment? simpleComment = await db.Comments.Include(c => c.User).ProjectToType<SimpleComment>().FirstOrDefaultAsync(c => c.Id == id);
            return simpleComment!;

        }
    }
}
