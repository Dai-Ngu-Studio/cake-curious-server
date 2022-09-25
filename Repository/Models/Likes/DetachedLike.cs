
namespace Repository.Models.Likes
{
    public class DetachedLike
    {
        public Guid? Id { get; set; }
        public string? UserId { get; set; }
        public Guid? RecipeId { get; set; }
    }
}
