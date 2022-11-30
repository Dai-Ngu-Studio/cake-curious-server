using Repository.Models.Users;

namespace Repository.Models.Comments
{
    public class RecipeComment
    {
        public Guid? Id { get; set; }
        public SimpleUser? User { get; set; }
        public string? Content { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public Guid? RootId { get; set; }
        public int? Depth { get; set; }
        public IEnumerable<RecipeCommentMedia>? Images { get; set; }
        public int? RepliesCount { get; set; }
    }
}
