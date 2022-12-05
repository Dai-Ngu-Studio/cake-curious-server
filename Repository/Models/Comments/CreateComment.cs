using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Comments
{
    public class CreateComment
    {
        public Guid? RecipeId { get; set; }

        [Required]
        public string? Content { get; set; }
        public Guid? RootId { get; set; }
        public IEnumerable<CreateCommentMedia>? Images { get; set; }
    }
}
