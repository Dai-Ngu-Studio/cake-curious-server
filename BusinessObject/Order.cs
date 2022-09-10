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
        public Guid Id { get; set; }

        [Column("buyer_id", TypeName = "varchar(128)")]
        public string? BuyerId { get; set; }

        [ForeignKey("BuyerId")]
        public User? Buyer { get; set; }

        [Column("store_id")]
        public Guid? StoreId { get; set; }

        [ForeignKey("StoreId")]
        public Store? Store { get; set; }

        [Column("subtotal", TypeName = "decimal(20,4)")]
        public decimal? Subtotal { get; set; }

        [Column("order_date", TypeName = "datetime2(7)")]
        public DateTime? OrderDate { get; set; }

        [Column("processed_date", TypeName = "datetime2(7)")]
        public DateTime? ProcessedDate { get; set; }

        [Column("completed_date", TypeName = "datetime2(7)")]
        public DateTime? CompletedDate { get; set; }

        [Column("coupon", TypeName = "varchar(24)")]
        public string? Coupon { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [InverseProperty("Order")]
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
