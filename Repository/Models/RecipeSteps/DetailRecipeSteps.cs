namespace Repository.Models.RecipeSteps
{
    public class DetailRecipeSteps
    {
        public Queue<DetailRecipeStep>? RecipeSteps { get; set; } = new Queue<DetailRecipeStep>();
    }
}
