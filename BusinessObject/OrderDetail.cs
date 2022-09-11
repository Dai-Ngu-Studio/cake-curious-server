using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("OrderDetail")]
    public class OrderDetail
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("order_id")]
        public Guid? OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [Column("product_id")]
        public Guid? ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Column("product_name", TypeName = "nvarchar(256)")]
        public string? ProductName { get; set; }

        [Column("amount")]
        public int? Amount { get; set; }

        [Column("price", TypeName = "decimal(20,4)")]
        public decimal? Price { get; set; }

    }
}
