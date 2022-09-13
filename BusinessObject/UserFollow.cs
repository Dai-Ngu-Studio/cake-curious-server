using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("UserFollow")]
    public class UserFollow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("user_id", TypeName = "varchar(128)")]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Column("follower_id", TypeName = "varchar(128)")]
        public string? FollowerId { get; set; }

        [ForeignKey("FollowerId")]
        public User? Follower { get; set; }
    }
}
