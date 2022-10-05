using BusinessObject;
using Repository.Models.Comments;

namespace Repository.Interfaces
{
    public interface ICommentRepository
    {
        public IEnumerable<RecipeComment> GetCommentsForRecipe(Guid recipeId);
        public Task<Comment?> GetCommentReadonly(Guid id);
        public Task<RecipeComment?> GetRecipeComment(Guid id);
        public Task Add(Comment comment);
        public Task<int> Update(Guid id, UpdateComment updateComment);
    }
}
