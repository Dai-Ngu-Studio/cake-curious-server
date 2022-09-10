using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("ProductType")]
    public class ProductType
    {
        public ProductType()
        {
            Products = new HashSet<Product>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name", TypeName = "nvarchar(256)")]
        public string? Name { get; set; }

        [InverseProperty("ProductType")]
        public ICollection<Product>? Products { get; set; }
    }
}
