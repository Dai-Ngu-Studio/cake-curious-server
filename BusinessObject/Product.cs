using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("Product")]
    public class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("store_id")]
        public Guid? StoreId { get; set; }

        [ForeignKey("StoreId")]
        public Store? Store { get; set; }

        [Column("type_id")]
        public Guid? ProductTypeId { get; set; }

        [ForeignKey("TypeId")]
        public ProductType? ProductType { get; set; }

        [Column("name", TypeName = "nvarchar(256)")]
        public string? Name { get; set; }

        [Column("description", TypeName = "nvarchar(512)")]
        public string? Description { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }

        [Column("price", TypeName = "decimal(20,4)")]
        public decimal? Price { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [InverseProperty("Product")]
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
