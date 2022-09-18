using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("Comment")]
    public class Comment
    {
        public Comment()
        {
            Images = new HashSet<CommentMedia>();
            Replies = new HashSet<Comment>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("user_id", TypeName = "varchar(128)")]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Column("recipe_id")]
        public Guid? RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; set; }

        [Column("content", TypeName = "nvarchar(512)")]
        public string? Content { get; set; }

        [Column("submitted_date", TypeName = "datetime2(7)")]
        public DateTime? SubmittedDate { get; set; }

        [Column("root_id")]
        public Guid? RootId { get; set; }

        [ForeignKey("RootId")]
        public Comment? Root { get; set; }

        [Column("depth")]
        public int? Depth { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [InverseProperty("Comment")]
        public ICollection<CommentMedia>? Images { get; set; }

        [InverseProperty("Root")]
        public ICollection<Comment>? Replies { get; set; }

    }
}
