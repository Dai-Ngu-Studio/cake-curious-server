using Repository.Models.Users;

namespace Repository.Models.Stores
{
    public class CartStore
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public int? Status { get; set; }
        public StatusOnlyUser? User { get; set; }
    }
}
