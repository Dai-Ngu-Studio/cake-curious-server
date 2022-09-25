using Repository.Models.RecipeMaterials;

namespace Repository.Models.RecipeSteps
{
    public class DetailRecipeStep
    {
        public Guid? Id { get; set; }
        public int? StepNumber { get; set; }
        public string? Content { get; set; }
        public int? StepTimestamp { get; set; }
        public IEnumerable<DetachedRecipeMaterial>? Ingredients { get; set; }
        public IEnumerable<DetachedRecipeMaterial>? Equipment { get; set; }
    }
}
