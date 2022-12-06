using BusinessObject;
using Repository.Models.Users;


namespace Repository.Models.Reports
{
    public class StaffDashboardReport
    {
        public Guid Id { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public ReportCategory? ReportCategory { get; set; }
        public Guid? ItemId { get; set; }
        public int? ItemType { get; set; }      
        public SimpleUser? Reporter { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public SimpleUser? Staff { get; set; }   
        public SimpleUser? ReportedUser { get; set; }
        public int Status { get; set; }
    }
}
