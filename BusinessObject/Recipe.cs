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
            RecipeMaterials = new HashSet<RecipeMaterial>();
            RecipeMedia = new HashSet<RecipeMedia>();
            RecipeSteps = new HashSet<RecipeStep>();
            Comments = new HashSet<Comment>();
            Likes = new HashSet<Like>();
            Bookmarks = new HashSet<Bookmark>();
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid? Id { get; set; }

        [Column("user_id", TypeName = "varchar(128)")]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Column("name", TypeName = "nvarchar(256)")]
        public string? Name { get; set; }

        [Column("description", TypeName = "nvarchar(512)")]
        public string? Description { get; set; }

        [Column("serving_size")]
        public int? ServingSize { get; set; }

        [Column("photo_url", TypeName = "nvarchar(2048)")]
        public string? PhotoUrl { get; set; }

        [Column("cook_time")]
        public int? CookTime { get; set; }

        [Column("published_date", TypeName = "datetime2(7)")]
        public DateTime? PublishedDate { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<RecipeHasCategory>? HasCategories { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<RecipeMaterial>? RecipeMaterials { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<RecipeMedia>? RecipeMedia { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<RecipeStep>? RecipeSteps { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<Comment>? Comments { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<Like>? Likes { get; set; }

        [InverseProperty("Recipe")]
        public ICollection<Bookmark>? Bookmarks { get; set; }
    }
}
