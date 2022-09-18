using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("Store")]
    public class Store
    {
        public Store()
        {
            Products = new HashSet<Product>();
            Orders = new HashSet<Order>();
            Coupons = new HashSet<Coupon>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("user_id", TypeName = "varchar(128)")]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Column("name", TypeName = "nvarchar(128)")]
        public string? Name { get; set; }

        [Column("description", TypeName = "nvarchar(512)")]
        public string? Description { get; set; }

        [Column("longitude", TypeName = "decimal(12,8)")]
        public decimal? Longitude { get; set; }

        [Column("latitude", TypeName = "decimal(12,8)")]
        public decimal? Latitude { get; set; }

        [Column("photo_url", TypeName = "nvarchar(2048)")]
        public string? PhotoUrl { get; set; }

        [Column("address", TypeName = "nvarchar(512)")]
        public string? Address { get; set; }

        [Column("rating", TypeName = "decimal(5,2)")]
        public decimal? Rating { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [InverseProperty("Store")]
        public ICollection<Product>? Products { get; set; }

        [InverseProperty("Store")]
        public ICollection<Order>? Orders { get; set; }

        [InverseProperty("Store")]
        public ICollection<Coupon>? Coupons { get; set; }
    }
}
