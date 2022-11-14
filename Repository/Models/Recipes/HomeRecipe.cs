using Repository.Models.Users;

namespace Repository.Models.Recipes
{
    public class HomeRecipe
    {
        public Guid? Id { get; set; }
        public SimpleUser? User { get; set; }
        public string? Name { get; set; }
        public int? ServingSize { get; set; }
        public string? PhotoUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int? CookTime { get; set; }
        public int? Likes { get; set; }
    }
}
