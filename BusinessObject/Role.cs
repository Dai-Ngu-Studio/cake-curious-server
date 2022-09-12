using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("Role")]
    public class Role
    {
        public Role()
        {
            HasUsers = new HashSet<UserHasRole>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public int? Id { get; set; }

        [Column("name", TypeName = "varchar(256)")]
        public string? Name { get; set; }

        [Column("short_name", TypeName = "varchar(128)")]
        public string? ShortName { get; set; }

        [InverseProperty("Role")]
        public ICollection<UserHasRole>? HasUsers { get; set; }

    }
}
