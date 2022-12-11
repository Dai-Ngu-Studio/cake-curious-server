using BusinessObject;
using Mapster;
using Repository.Constants.Recipes;
using Repository.Constants.Users;
using Repository.Models.Users;

namespace Repository.Configuration.Mappings
{
    public static class UserMapConfiguration
    {
        public static void RegisterUserMapping()
        {
            TypeAdapterConfig<UserFollow, FollowingUser>
                .NewConfig()
                .Map(dest => dest.Id, src => src.UserId)
                .Map(dest => dest.PhotoUrl, src => src.User!.PhotoUrl)
                .Map(dest => dest.DisplayName, src => src.User!.DisplayName)
                .Map(dest => dest.IsFollowedByCurrentUser, src => src.User!.Followers!
                    .Any(x => x.FollowerId == (string)MapContext.Current!.Parameters["userId"]));

            TypeAdapterConfig<UserFollow, FollowerUser>
                .NewConfig()
                .Map(dest => dest.Id, src => src.FollowerId)
                .Map(dest => dest.PhotoUrl, src => src.Follower!.PhotoUrl)
                .Map(dest => dest.DisplayName, src => src.Follower!.DisplayName)
                .Map(dest => dest.IsFollowedByCurrentUser, src => src.Follower!.Followers!
                    .Any(x => x.FollowerId == (string)MapContext.Current!.Parameters["userId"]));

            TypeAdapterConfig<User, ProfileUser>
                .NewConfig()
                .Map(dest => dest.Followers, src => src.Followers!
                    .Where(x => x.Follower!.Status == (int)UserStatusEnum.Active).Count())
                .Map(dest => dest.Following, src => src.Followings!
                    .Where(x => x.Follower!.Status == (int)UserStatusEnum.Active).Count())
                .Map(dest => dest.Recipes, src => src.Recipes!
                    .Where(x => x.Status == (int)RecipeStatusEnum.Active)
                    .Where(x => x.User!.Status == (int)UserStatusEnum.Active)
                    .Count())
                .Map(dest => dest.IsFollowedByCurrentUser, src => src.Followers!
                    .Any(x => x.FollowerId == (string)MapContext.Current!.Parameters["userId"]));
        }
    }
}
