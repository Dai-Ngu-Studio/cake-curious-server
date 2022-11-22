using BusinessObject;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Comments;
using Repository.Constants.Reports;
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

        public async Task<RecipeComment?> GetRecipeComment(Guid id)
        {
            var db = new CakeCuriousDbContext();
            return await db.Comments.AsNoTracking().Where(x => x.Id == id).ProjectToType<RecipeComment>().FirstOrDefaultAsync();
        }

        public async Task<int> Update(Guid id, UpdateComment updateComment)
        {
            var db = new CakeCuriousDbContext();
            string query = $"update [Comment] set [Comment].content = @commentContent where [Comment].id = '{id}'";
            using (var connection = db.Database.GetDbConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var cmd = connection.CreateCommand();
                        cmd.Transaction = transaction;
                        cmd.Connection = connection;
                        cmd.CommandText = query;
                        cmd.Parameters.Add(new SqlParameter("@commentContent", updateComment.Content));
                        var rows = await cmd.ExecuteNonQueryAsync();
                        await transaction.CommitAsync();
                        return rows;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        return 0;
                    }
                }
            }
        }

        public async Task<int> Delete(Guid id)
        {
            var db = new CakeCuriousDbContext();
            string query = $"update [Comment] set [Comment].status = {(int)CommentStatusEnum.Inactive} where [Comment].id = '{id}'";
            using (var connection = db.Database.GetDbConnection())
            {
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var cmd = connection.CreateCommand();
                        cmd.Transaction = transaction;
                        cmd.Connection = connection;
                        cmd.CommandText = query;
                        var rows = await cmd.ExecuteNonQueryAsync();
                        await transaction.CommitAsync();
                        return rows;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        return 0;
                    }
                }
            }
        }

        public async Task<int> CountCommentsForRecipe(Guid recipeId)
        {
            var db = new CakeCuriousDbContext();
            return await db.Comments
                .AsNoTracking()
                .Where(x => x.RecipeId == recipeId)
                .Where(x => x.RootId == null)
                .Where(x => x.Status == (int)CommentStatusEnum.Active)
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
                .Skip(skip)
                .Take(take)
                .ProjectToType<RecipeComment>();
        }
        public List<SimpleCommentForReportList>? FilterByStatusActiveList(List<SimpleCommentForReportList>? isReportedComments)
        {
            return isReportedComments!.Where(p => p.Status == (int)CommentStatusEnum.Active).ToList();
        }

        public List<SimpleCommentForReportList>? FilterByStatusInactive(List<SimpleCommentForReportList>? isReportedComments)
        {
            return isReportedComments!.Where(p => p.Status == (int)CommentStatusEnum.Inactive).ToList();
        }
        public async Task<IEnumerable<SimpleCommentForReportList>> GetReportedCommments(string? filter, int page, int size)
        {
            var db = new CakeCuriousDbContext();
            IEnumerable<ViolationReport> reportsCommentType = await db.ViolationReports.Where(report => report.ItemType == (int)ReportTypeEnum.Comment).GroupBy(x => x.ItemId).Select(d => d.First()).ToListAsync();
            List<SimpleCommentForReportList>? isReportedComments = new List<SimpleCommentForReportList>();
            if (reportsCommentType.Count() > 0)
                foreach (var report in reportsCommentType)
                {
                    isReportedComments!.Add((await db.Comments.ProjectToType<SimpleCommentForReportList>().FirstOrDefaultAsync(r => r.Id == report!.ItemId))!);
                }
            //filter
            if (filter != null && filter == CommentStatusEnum.Active.ToString())
            {
                isReportedComments = FilterByStatusActiveList(isReportedComments);
            }
            else if (filter != null && filter == CommentStatusEnum.Inactive.ToString())
            {
                isReportedComments = FilterByStatusInactive(isReportedComments);
            }
            return isReportedComments!.Skip((page - 1) * size)
                                .Take(size).ToList();
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
                isReportedComments = FilterByStatusActiveList(isReportedComments);
            }
            else if (filter != null && filter == CommentStatusEnum.Inactive.ToString())
            {
                isReportedComments = FilterByStatusInactive(isReportedComments);
            }
            return isReportedComments!.Count();
        }
    }
}
