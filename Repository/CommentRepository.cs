using BusinessObject;
using Mapster;
using Repository.Interfaces;
using Repository.Models.Comments;

namespace Repository
{
    public class CommentRepository : ICommentRepository
    {
        public ICollection<RecipeComment> GetCommentsForRecipe(Guid recipeId)
        {
            var db = new CakeCuriousDbContext();
            return db.Comments
                .Where(x => x.RecipeId == recipeId && x.Depth == 0)
                .ProjectToType<RecipeComment>()
                .ToList();
        }
    }
}
