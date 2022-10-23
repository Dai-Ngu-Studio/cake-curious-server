namespace Repository.Models.RecipeCategories
{
    public class DetachedRecipeCategory
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public int? RecipeCategoryGroupId { get; set; }
    }
}
