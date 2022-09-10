using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObject
{
    [Table("Recipe")]
    public class Recipe
    {
        public Recipe()
        {
            HasCategories = new HashSet<RecipeHasCategory>();
            BakingMaterials = new HashSet<RecipeBakingMaterial>();
            VisualMaterials = new HashSet<RecipeVisualMaterial>();
            Steps = new HashSet<RecipeStep>();
            Comments = new HashSet<Comment>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("author_id", TypeName = "varchar(128)")]
        public string? AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public User? Author { get; set; }

        [Column("name", TypeName = "nvarchar(256)")]
        public string? Name { get; set; }

        [Column("description", TypeName = "nvarchar(512)")]
        public string? Description { get; set; }

        [Column("photo_url", TypeName = "nvarchar(max)")]
        public string? PhotoUrl { get; set; }

        [Column("time_needed", TypeName = "decimal(9,4)")]
        public decimal? TimeNeeded { get; set; }

        [Column("published_date", TypeName = "datetime2(7)")]
        public DateTime? PublishedDate { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<RecipeHasCategory>? HasCategories { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<RecipeBakingMaterial>? BakingMaterials { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<RecipeVisualMaterial>? VisualMaterials { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<RecipeStep>? Steps { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<Comment>? Comments { get; set; }
    }
}
