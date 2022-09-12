using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("User")]
    public class User
    {
        public User()
        {
            HasRoles = new HashSet<UserHasRole>();
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

        [Column("password", TypeName = "nvarchar(max)")]
        public string? Password { get; set; }

        [Column("photo_url", TypeName = "nvarchar(max)")]
        public string? PhotoUrl { get; set; }

        [Column("gender", TypeName = "nvarchar(16)")]
        public string? Gender { get; set; }

        [Column("date_of_birth", TypeName = "datetime2(7)")]
        public DateTime? DateOfBirth { get; set; }

        [Column("address", TypeName = "nvarchar(512)")]
        public string? Address { get; set; }

        [Column("citizenship_number", TypeName = "varchar(24)")]
        public string? CitizenshipNumber { get; set; }

        [Column("citizenship_date", TypeName = "datetime2(7)")]
        public DateTime? CitizenshipDate { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [InverseProperty("User")]
        public ICollection<UserHasRole>? HasRoles { get; set; }

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
