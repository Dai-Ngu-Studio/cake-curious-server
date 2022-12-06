

namespace Repository.Models.Users
{
    public class AdminDashboardUserPage
    {
        public IEnumerable<AdminDashboardUser>? Users { get; set; }
        public int TotalPage { get; set; } 
    }
}
