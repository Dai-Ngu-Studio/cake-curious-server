namespace Repository.Models.Users
{
    public class ElasticsearchUser
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public int[]? Roles { get; set; }
    }
}
