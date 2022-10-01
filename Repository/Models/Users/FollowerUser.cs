namespace Repository.Models.Users
{
    public class FollowerUser
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
        public bool? IsFollowedByCurrentUser { get; set; }
    }
}
