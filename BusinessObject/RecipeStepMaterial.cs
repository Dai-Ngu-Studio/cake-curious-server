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
        public Guid? StepId { get; set; }

        [ForeignKey("StepId")]
        public RecipeStep? RecipeStep { get; set; }

        [Column("material_name", TypeName = "nvarchar(256)")]
        public string? MaterialName { get; set; }

        [Column("amount", TypeName = "nvarchar(64)")]
        public string? Amount { get; set; }
    }
}
