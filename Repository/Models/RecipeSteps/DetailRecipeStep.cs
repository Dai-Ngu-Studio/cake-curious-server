using Repository.Models.RecipeMaterials;

namespace Repository.Models.RecipeSteps
{
    public class DetailRecipeStep
    {
        public Guid? Id { get; set; }
        public int? StepNumber { get; set; }
        public string? Content { get; set; }
        public int? StepTimestamp { get; set; }
        public ICollection<DetachedRecipeMaterial>? Ingredients { get; set; }
        public ICollection<DetachedRecipeMaterial>? Equipment { get; set; }
    }
}
