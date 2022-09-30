namespace Repository.Models.Bookmarks
{
    public class DetachedBookmark
    {
        public Guid? Id { get; set; }
        public string? UserId { get; set; }
        public Guid? RecipeId { get; set; }
    }
}
