using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("RecipeStep")]
    public class RecipeStep
    {
        public RecipeStep()
        {
            StepMaterials = new HashSet<RecipeStepMaterial>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("recipe_id")]
        public Guid? RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; set; }

        [Column("step_number")]
        public int? StepNumber { get; set; }

        [Column("content", TypeName = "nvarchar(512)")]
        public string? Content { get; set; }

        [Column("step_timestamp")]
        public int? StepTimestamp { get; set; }

        [InverseProperty("RecipeStep")]
        public ICollection<RecipeStepMaterial>? StepMaterials { get; set; }
    }
}
