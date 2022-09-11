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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("name", TypeName = "nvarchar(128)")]
        public string? Name { get; set; }

        [InverseProperty("Category")]
        public ICollection<RecipeHasCategory>? HasRecipes { get; set; }
    }
}
