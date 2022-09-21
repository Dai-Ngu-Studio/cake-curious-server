using Repository.Models.Users;

namespace Repository.Models.Recipes
{
    public class FollowRecipe
    {
        public Guid? Id { get; set; }
        public SimpleUser? User { get; set; }
        public string? Name { get; set; }
        public int? ServingSize { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? CookTime { get; set; }
        public int? Likes { get; set; }
    }
}
