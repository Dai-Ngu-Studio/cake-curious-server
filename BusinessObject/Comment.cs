using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("Comment")]
    public class Comment
    {
        public Comment()
        {
            Images = new HashSet<CommentImage>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("author_id", TypeName = "varchar(128)")]
        public string? AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public User? Author { get; set; }

        [Column("recipe_id")]
        public Guid? RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; set; }

        [Column("content", TypeName = "nvarchar(512)")]
        public string? Content { get; set; }

        [Column("submitted_date", TypeName = "datetime2(7)")]
        public DateTime? SubmittedDate { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [InverseProperty("Comment")]
        public ICollection<CommentImage>? Images { get; set; }

    }
}
