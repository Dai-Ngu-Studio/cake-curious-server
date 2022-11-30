using Repository.Models.Users;

namespace Repository.Models.Stores
{
    public class BundleStore
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public int? Status { get; set; }
    }
}
