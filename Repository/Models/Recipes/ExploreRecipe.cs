namespace Repository.Models.Recipes
{
    public class ExploreRecipe
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? PhotoUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? Key { get; set; }
    }
}
