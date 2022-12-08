using Repository.Models.RecipeCategoryGroups;
using Repository.Models.RecipeHasCategories;
using Repository.Models.RecipeMaterials;
using Repository.Models.RecipeMedia;
using Repository.Models.RecipeSteps;
using Repository.Models.Users;

namespace Repository.Models.Recipes
{
    public class EngEditRecipe
    {
        public Guid? Id { get; set; }
        public SimpleUser? User { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? ServingSize { get; set; }
        public string? PhotoUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? CookTime { get; set; }
        public string? ShareUrl { get; set; }
        public IEnumerable<CreateRecipeMaterial> Ingredients { get; set; } = new List<CreateRecipeMaterial>();
        public IEnumerable<CreateRecipeMaterial> Equipment { get; set; } = new List<CreateRecipeMaterial>();
        public IEnumerable<CreateRecipeStep> RecipeSteps { get; set; } = new List<CreateRecipeStep>();
        public IEnumerable<CreateRecipeHasCategory>? HasCategories { get; set; }
        public IEnumerable<EngDetachedRecipeCategoryGroup>? RecipeCategoryGroups { get; set; }
        public IEnumerable<CreateRecipeMedia>? RecipeMedia { get; set; }
    }
}
