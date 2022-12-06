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

        [Column("product_type")]
        public int? ProductType { get; set; }

        [Column("category_id")]
        public int? ProductCategoryId { get; set; }

        [ForeignKey("ProductCategoryId")]
        public ProductCategory? ProductCategory { get; set; }

        [Column("name", TypeName = "nvarchar(256)")]
        public string? Name { get; set; }

        [Column("description", TypeName = "nvarchar(512)")]
        public string? Description { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }

        [Column("price", TypeName = "decimal(20,4)")]
        public decimal? Price { get; set; }

        [Column("photo_url", TypeName = "nvarchar(2048)")]
        public string? PhotoUrl { get; set; }

        [Column("share_url", TypeName = "nvarchar(2048)")]
        public string? ShareUrl { get; set; }

        [Column("rating", TypeName = "decimal(5,2)")]
        public decimal? Rating { get; set; }

        [Column("last_updated", TypeName = "datetime2(7)")]
        public DateTime? LastUpdated { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [InverseProperty("Product")]
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
