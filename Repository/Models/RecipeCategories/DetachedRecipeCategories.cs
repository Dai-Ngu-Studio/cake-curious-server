namespace Repository.Models.RecipeCategories
{
    public class DetachedRecipeCategories<T>
    {
        public IEnumerable<T>? RecipeCategories { get; set; }
    }
}
