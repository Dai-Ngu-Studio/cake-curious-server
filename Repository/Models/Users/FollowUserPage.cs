namespace Repository.Models.Users
{
    public class FollowUserPage
    {
        public int? TotalPages { get; set; }
        public IEnumerable<FollowerUser>? Followers { get; set; }
        public IEnumerable<FollowingUser>? Followings { get; set; }
    }
}
