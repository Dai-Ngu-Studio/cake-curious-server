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
            Orders = new HashSet<Order>();
            ViolationReports = new HashSet<ViolationReport>();
            ResolvedViolationReports = new HashSet<ViolationReport>();
            Likes = new HashSet<Like>();
            Bookmarks = new HashSet<Bookmark>();
            Followers = new HashSet<UserFollow>();
            Followings = new HashSet<UserFollow>();
            DeactivateReasons = new HashSet<DeactivateReason>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id", TypeName = "varchar(128)")]
        public string? Id { get; set; } = null!;

        [Column("email", TypeName = "nvarchar(256)")]
        public string? Email { get; set; }

        [Column("username", TypeName = "nvarchar(128)")]
        public string? Username { get; set; }

        [Column("display_name", TypeName = "nvarchar(64)")]
        public string? DisplayName { get; set; }

        [Column("photo_url", TypeName = "nvarchar(2048)")]
        public string? PhotoUrl { get; set; }

        [Column("phone_number", TypeName = "nvarchar(24)")]
        public string? PhoneNumber { get; set; }

        [Column("full_name", TypeName = "nvarchar(256)")]
        public string? FullName { get; set; }

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

        [Column("store_id")]
        public Guid? StoreId { get; set; }

        [ForeignKey("StoreId")]
        public Store? Store { get; set; }

        [Column("created_date", TypeName = "datetime2(7)")]
        public DateTime? CreatedDate { get; set; }

        [Column("share_url", TypeName = "nvarchar(2048)")]
        public string? ShareUrl { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [InverseProperty("User")]
        public ICollection<UserHasRole>? HasRoles { get; set; }

        [InverseProperty("User")]
        public ICollection<UserDevice>? UserDevices { get; set; }

        [InverseProperty("User")]
        public ICollection<Recipe>? Recipes { get; set; }

        [InverseProperty("User")]
        public ICollection<Comment>? Comments { get; set; }

        [InverseProperty("User")]
        public ICollection<Order>? Orders { get; set; }

        [InverseProperty("Reporter")]
        public ICollection<ViolationReport>? ViolationReports { get; set; }

        [InverseProperty("Staff")]
        public ICollection<ViolationReport>? ResolvedViolationReports { get; set; }

        [InverseProperty("User")]
        public ICollection<Like>? Likes { get; set; }

        [InverseProperty("User")]
        public ICollection<Bookmark>? Bookmarks { get; set; }

        [InverseProperty("User")]
        public ICollection<UserFollow>? Followers { get; set; }

        [InverseProperty("Follower")]
        public ICollection<UserFollow>? Followings { get; set; }
		
        [InverseProperty("Staff")]
        public ICollection<DeactivateReason>? DeactivateReasons { get; set; }

    }
}
