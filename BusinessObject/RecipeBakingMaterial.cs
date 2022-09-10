using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("RecipeBakingMaterial")]
    public class RecipeBakingMaterial
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("recipe_id")]
        public Guid? RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; set; }

        [Column("material_name", TypeName = "nvarchar(256)")]
        public string? MaterialName { get; set; }

        [Column("amount", TypeName = "nvarchar(256)")]
        public string? Amount { get; set; }
    }
}
