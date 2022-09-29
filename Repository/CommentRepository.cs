using BusinessObject;
using Mapster;
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
    }
}
