using BusinessObject;
using Mapster;
using Repository.Models.Comments;

namespace Repository.Configuration.Mappings
{
    public static class CommentMapConfiguration
    {
        public static void RegisterCommentMapping()
        {
            TypeAdapterConfig<Comment, RecipeComment>
                .NewConfig()
                .MaxDepth(5);
        }
    }
}
