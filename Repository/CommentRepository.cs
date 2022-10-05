using BusinessObject;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Repository.Constants.Comments;
using Repository.Interfaces;
using Repository.Models.Comments;

namespace Repository
{
    public class CommentRepository : ICommentRepository
    {
        public IEnumerable<RecipeComment> GetCommentsForRecipe(Guid recipeId)
        {
            var db = new CakeCuriousDbContext();
            return db.Comments
                .Where(x => x.RecipeId == recipeId && x.RootId == null)
                .Where(x => x.Status == (int)CommentStatusEnum.Active)
                .ProjectToType<RecipeComment>();
        }

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
            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                string query = $"update [Comment] set [Comment].content = N'{updateComment.Content}' where [Comment].id = '{id}'";
                var rows = await db.Database.ExecuteSqlRawAsync(query);
                await transaction.CommitAsync();
                return rows;
            }
        }
    }
}
