using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("CommentMedia")]
    public class CommentMedia
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("comment_id")]
        public Guid? CommentId { get; set; }

        [ForeignKey("CommentId")]
        public Comment? Comment { get; set; }

        [Column("media_url", TypeName = "nvarchar(2048)")]
        public string? MediaUrl { get; set; }
    }
}
