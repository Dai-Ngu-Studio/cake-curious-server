using Repository.Models.Users;


namespace Repository.Models.Stores
{
    public class AdminDashboardStore
    {
        public Guid? Id { get; set; }
        public int? Status { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? Rating { get; set; }
        public string? Address  { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public SimpleUser? User { get; set; }

    }
}
