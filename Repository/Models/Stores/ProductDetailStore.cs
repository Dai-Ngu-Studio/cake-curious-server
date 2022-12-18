using Repository.Models.Users;

namespace Repository.Models.Stores
{
    public class ProductDetailStore
    {
        public Guid? Id { get; set; }
        public string? UserId { get; set; }
        public StatusOnlyUser? User { get; set; }
        public string? Name { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? Rating { get; set; }
        public int? Status { get; set; }
    }
}
