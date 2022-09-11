using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("User")]
    public class User
    {
        public User()
        {
            UserDevices = new HashSet<UserDevice>();
            Recipes = new HashSet<Recipe>();
            Comments = new HashSet<Comment>();
            Stores = new HashSet<Store>();
            Orders = new HashSet<Order>();
            ViolationReports = new HashSet<ViolationReport>();
            ResolvedViolationReports = new HashSet<ViolationReport>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id", TypeName = "varchar(128)")]
        public string? Id { get; set; } = null!;

        [Column("email", TypeName = "nvarchar(256)")]
        public string? Email { get; set; }

        [Column("display_name", TypeName = "nvarchar(256)")]
        public string? DisplayName { get; set; }

        [Column("photo_url", TypeName = "nvarchar(max)")]
        public string? PhotoUrl { get; set; }

        [Column("role_id")]
        public int? RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role? Role { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [InverseProperty("User")]
        public ICollection<UserDevice>? UserDevices { get; set; }

        [InverseProperty("Author")]
        public ICollection<Recipe>? Recipes { get; set; }

        [InverseProperty("Author")]
        public ICollection<Comment>? Comments { get; set; }

        [InverseProperty("Owner")]
        public ICollection<Store>? Stores { get; set; }

        [InverseProperty("Buyer")]
        public ICollection<Order>? Orders { get; set; }

        [InverseProperty("Reporter")]
        public ICollection<ViolationReport>? ViolationReports { get; set; }

        [InverseProperty("Staff")]
        public ICollection<ViolationReport>? ResolvedViolationReports { get; set; }

    }
}
