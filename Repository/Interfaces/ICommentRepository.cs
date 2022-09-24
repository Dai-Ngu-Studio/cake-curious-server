using Repository.Models.Comments;

namespace Repository.Interfaces
{
    public interface ICommentRepository
    {
        public ICollection<RecipeComment> GetCommentsForRecipe(Guid recipeId);
    }
}
