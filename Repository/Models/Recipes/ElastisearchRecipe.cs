namespace Repository.Models.Recipes
{
    public class ElastisearchRecipe
    {
        public Guid? Id { get; set; }
        public string[]? Name { get; set; } 
        public string[]? Materials { get; set; }
        public int[]? Categories { get; set; }
    }
}
