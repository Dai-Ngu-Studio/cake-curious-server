using BusinessObject;
using Mapster;
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
