using BusinessObject;

namespace Repository.Models.Users
{
    public class DetachedUser
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? CitizenshipNumber { get; set; }
        public DateTime? CitizenshipDate { get; set; }
        public Guid? StoreId { get; set; }
        public int? Status { get; set; }
        public IEnumerable<UserHasRole>? HasRoles { get; set; }
    }
}
