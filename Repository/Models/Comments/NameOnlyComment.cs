namespace Repository.Models.Comments
{
    public class NameOnlyComment
    {
        public Guid? Id { get; set; }
        public string? UserId { get; set; }
        public string? UserDisplayName { get; set; }
    }
}
