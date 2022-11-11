namespace Repository.Models.Comments
{
    public class CommentPage
    {
        public int? TotalPages { get; set; }
        public IEnumerable<RecipeComment>? Comments { get; set; }
    }
}
