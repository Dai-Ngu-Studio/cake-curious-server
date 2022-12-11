using Repository.Models.Users;

namespace Repository.Models.DeactivateReasons
{
    public class DetailDeactivateReason
    {
        public Guid? Id { get; set; }
        public Guid? ItemId { get; set; }
        public int? ItemType { get; set; }
        public string? StaffId { get; set; }
        public SimpleUser? Staff { get; set; }
        public string? Reason { get; set; }
        public DateTime? DeactivateDate { get; set; }
    }
}
