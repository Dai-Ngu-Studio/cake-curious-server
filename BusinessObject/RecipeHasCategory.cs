using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("RecipeHasCategory")]
    public class RecipeHasCategory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("recipe_id")]
        public Guid? RecipeId { get; set; }

        [ForeignKey("RecipeId")]
        public Recipe? Recipe { get; set; }

        [Column("category_id")]
        public int? RecipeCategoryId { get; set; }

        [ForeignKey("RecipeCategoryId")]
        public RecipeCategory? RecipeCategory { get; set; }

    }
}
