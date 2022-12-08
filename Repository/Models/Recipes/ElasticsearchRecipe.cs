namespace Repository.Models.Recipes
{
    public class ElasticsearchRecipe
    {
        public Guid? Id { get; set; }
        public string[]? Name { get; set; } 
        public string[]? Materials { get; set; }
        public string[]? Equipment { get; set; }
        public int[]? Categories { get; set; }
    }
}
