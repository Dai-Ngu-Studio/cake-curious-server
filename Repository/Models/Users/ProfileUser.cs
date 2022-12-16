namespace Repository.Models.Users
{
    public class ProfileUser
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }   
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
        public int? Followers { get; set; }
        public int? Following { get; set; }
        public int? Recipes { get; set; }
        public bool? IsFollowedByCurrentUser { get; set; }
        public string? ShareUrl { get; set; }
        public int? Status { get; set; }
    }
}
