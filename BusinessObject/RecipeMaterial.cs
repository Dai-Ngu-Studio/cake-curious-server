using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("RecipeMaterial")]
    public class RecipeMaterial
    {
        public RecipeMaterial()
        {
            RecipeStepMaterials = new HashSet<RecipeStepMaterial>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("recipe_id")]
        public Guid? RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; set; }

        [Column("material_type")]
        public int? MaterialType { get; set; }

        [Column("material_name", TypeName = "nvarchar(64)")]
        public string? MaterialName { get; set; }

        [Column("amount", TypeName = "decimal(8,2)")]
        public decimal? Amount { get; set; }

        [Column("measurement", TypeName = "nvarchar(24)")]
        public string? Measurement { get; set; }

        [Column("color", TypeName = "varchar(8)")]
        public string? Color { get; set; }

        [InverseProperty("RecipeMaterial")]
        public ICollection<RecipeStepMaterial>? RecipeStepMaterials { get; set; }
    }
}
