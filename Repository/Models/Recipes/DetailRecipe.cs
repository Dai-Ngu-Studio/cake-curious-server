using Repository.Models.RecipeMaterials;
using Repository.Models.RecipeSteps;
using Repository.Models.Users;

namespace Repository.Models.Recipes
{
    public class DetailRecipe
    {
        public Guid? Id { get; set; }
        public SimpleUser? User { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? ServingSize { get; set; }
        public string? PhotoUrl { get; set; }
        public string? VideoUrl { get; set; }
        public int? CookTime { get; set; }
        public DateTime? PublishedDate { get; set; }
        public int? Likes { get; set; }
        public bool? IsLikedByCurrentUser { get; set; }
        public bool? IsBookmarkedByCurrentUser { get; set; }
        public string? ShareUrl { get; set; }
        public IEnumerable<DetachedRecipeMaterial>? Ingredients { get; set; }
        public IEnumerable<DetachedRecipeMaterial>? Equipment { get; set; }
        public IEnumerable<SimpleRecipeStep>? RecipeSteps { get; set; }
    }
}
