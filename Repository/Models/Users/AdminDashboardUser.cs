namespace Repository.Models.Users
{
    public class AdminDashboardUser
    {    
        public string? Id { get; set; } = null!;     
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public string? FullName { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<int>? Roles { get; set; } = new List<int>();
    }
}
