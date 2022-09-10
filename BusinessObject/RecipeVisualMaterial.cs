using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("RecipeVisualMaterial")]
    public class RecipeVisualMaterial
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("recipe_id")]
        public Guid? RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; set; }

        [Column("material_url", TypeName = "nvarchar(max)")]
        public string? MaterialUrl { get; set; }

        [Column("visual_type")]
        public int VisualType { get; set; }
    }
}
