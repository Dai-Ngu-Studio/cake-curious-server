namespace Repository.Models.Users
{
    public class SimpleUserPage
    {
        public int? TotalPages { get; set; }
        public IEnumerable<SimpleUser?>? Users { get; set; }
    }
}
