using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("UserDevice")]
    public class UserDevice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("token", TypeName = "varchar(1024)")]
        public string? Token { get; set; } = null!;

        [Column("user_id", TypeName = "varchar(128)")]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

    }
}
