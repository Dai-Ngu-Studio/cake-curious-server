using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("RecipeCategory")]
    public class RecipeCategory
    {
        public RecipeCategory()
        {
            HasRecipes = new HashSet<RecipeHasCategory>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public int? Id { get; set; }

        [Column("name", TypeName = "nvarchar(32)")]
        public string? Name { get; set; }

        [Column("lang_code", TypeName = "varchar(128)")]
        public string? LangCode { get; set; }

        [Column("group_id")]
        public int? RecipeCategoryGroupId { get; set; }

        [ForeignKey("RecipeCategoryGroupId")]
        public RecipeCategoryGroup? RecipeCategoryGroup { get; set; }

        [InverseProperty("RecipeCategory")]
        public ICollection<RecipeHasCategory>? HasRecipes { get; set; }
    }
}
