using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("Order")]
    public class Order
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("user_id", TypeName = "varchar(128)")]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Column("store_id")]
        public Guid? StoreId { get; set; }

        [ForeignKey("StoreId")]
        public Store? Store { get; set; }

        [Column("total", TypeName = "decimal(20,4)")]
        public decimal? Total { get; set; }

        [Column("address", TypeName = "nvarchar(512)")]
        public string? Address { get; set; }

        [Column("order_date", TypeName = "datetime2(7)")]
        public DateTime? OrderDate { get; set; }

        [Column("processed_date", TypeName = "datetime2(7)")]
        public DateTime? ProcessedDate { get; set; }

        [Column("completed_date", TypeName = "datetime2(7)")]
        public DateTime? CompletedDate { get; set; }

        [Column("coupon_id")]
        public Guid? CouponId { get; set; }

        [ForeignKey("CouponId")]
        public Coupon? Coupon { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [InverseProperty("Order")]
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
