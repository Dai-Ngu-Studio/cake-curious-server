using Repository.Models.Users;

namespace Repository.Models.Comments
{
    public class SimpleComment
    {
        public Guid? Id { get; set; }
        public SimpleUser? User { get; set; }
        public string? Content { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public IEnumerable<RecipeCommentMedia>? Images { get; set; }
    }
}
