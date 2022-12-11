using Repository.Models.DeactivateReasons;

namespace Repository.Models.Stores
{
    public class LoginStore
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? PhotoUrl { get; set; }
        public int? Status { get; set; }
        public DetailDeactivateReason? DeactivateReason { get; set; }
    }
}
