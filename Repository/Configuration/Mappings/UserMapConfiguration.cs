using BusinessObject;
using Mapster;
using Repository.Models.Users;

namespace Repository.Configuration.Mappings
{
    public static class UserMapConfiguration
    {
        public static void RegisterUserMapping()
        {
            TypeAdapterConfig<UserFollow, FollowUser>
                .NewConfig()
                .Map(dest => dest.Id, src => src.FollowerId)
                .Map(dest => dest.PhotoUrl, src => src.Follower!.PhotoUrl)
                .Map(dest => dest.DisplayName, src => src.Follower!.DisplayName)
                .Map(dest => dest.IsFollowedByCurrentUser, src => src.Follower!.Followers!
                    .Any(x => MapContext.Current!.Parameters["userId"] != null 
                    ? x.FollowerId == (string)MapContext.Current!.Parameters["userId"] 
                    : false));
        }
    }
}
