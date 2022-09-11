using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("CommentImage")]
    public class CommentImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("comment_id")]
        public Guid? CommentId { get; set; }

        [ForeignKey("CommentId")]
        public Comment? Comment { get; set; }

        [Column("image_url", TypeName = "nvarchar(max)")]
        public string? ImageUrl { get; set; }
    }
}
