namespace Repository.Models.Users
{
    public class FollowUser
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
        public bool? IsFollowedByCurrentUser { get; set; }
    }
}
