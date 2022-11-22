namespace Repository.Models.Users
{
    public class SimpleUser
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
        public double? Score { get; set; }
    }
}
