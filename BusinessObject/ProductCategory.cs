using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("ProductCategory")]
    public class ProductCategory
    {
        public ProductCategory()
        {
            Products = new HashSet<Product>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public int? Id { get; set; }

        [Column("name", TypeName = "nvarchar(48)")]
        public string? Name { get; set; }

        [Column("en_name", TypeName = "nvarchar(48)")]
        public string? EnName { get; set; }

        [InverseProperty("ProductCategory")]
        public ICollection<Product>? Products { get; set; }
    }
}
