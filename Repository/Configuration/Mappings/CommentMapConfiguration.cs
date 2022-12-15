using BusinessObject;
using Mapster;
using Repository.Constants.Comments;
using Repository.Constants.Users;
using Repository.Models.Comments;

namespace Repository.Configuration.Mappings
{
    public static class CommentMapConfiguration
    {
        public static void RegisterCommentMapping()
        {
            TypeAdapterConfig<Comment, RecipeComment>
                .NewConfig()
                .Map(dest => dest.RepliesCount, src => src.Replies!
                    .Where(x => x.Status == (int)CommentStatusEnum.Active)
                    .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                    .Count())
                .MaxDepth(5);

            TypeAdapterConfig<Comment, NameOnlyComment>
                .NewConfig()
                .Map(dest => dest.UserDisplayName, src => src.User!.DisplayName);
        }
    }
}
