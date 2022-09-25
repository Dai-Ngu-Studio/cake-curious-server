using Repository.Models.Comments;

namespace Repository.Interfaces
{
    public interface ICommentRepository
    {
        public IEnumerable<RecipeComment> GetCommentsForRecipe(Guid recipeId);
    }
}
