using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("RecipeStepMaterial")]
    public class RecipeStepMaterial
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("step_id")]
        public Guid? RecipeStepId { get; set; }

        [ForeignKey("RecipeStepId")]
        public RecipeStep? RecipeStep { get; set; }

        [Column("material_id")]
        public Guid? RecipeMaterialId { get; set; }

        [ForeignKey("RecipeMaterialId")]
        public RecipeMaterial? RecipeMaterial { get; set; }
    }
}
