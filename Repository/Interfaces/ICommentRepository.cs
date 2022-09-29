using BusinessObject;
using Repository.Models.Comments;

namespace Repository.Interfaces
{
    public interface ICommentRepository
    {
        public IEnumerable<RecipeComment> GetCommentsForRecipe(Guid recipeId);
        public Task Add(Comment comment);
    }
}
