using Repository.Models.RecipeStepMaterials;

namespace Repository.Models.RecipeSteps
{
    public class EditRecipeStep
    {
        public Guid? Id { get; set; }
        public int? StepNumber { get; set; }
        public string? Content { get; set; }
        public int? StepTimestamp { get; set; }
        public IEnumerable<CreateRecipeStepMaterial> RecipeStepMaterials { get; set; } = new List<CreateRecipeStepMaterial>();
    }
}
