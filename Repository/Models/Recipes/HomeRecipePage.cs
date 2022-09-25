namespace Repository.Models.Recipes
{
    public class HomeRecipePage
    {
        public int? TotalPages { get; set; }
        public IEnumerable<HomeRecipe>? Recipes { get; set; }
    }
}
