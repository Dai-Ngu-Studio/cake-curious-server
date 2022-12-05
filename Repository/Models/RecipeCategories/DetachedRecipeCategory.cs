namespace Repository.Models.RecipeCategories
{
    public class DetachedRecipeCategory
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public int? RecipeCategoryGroupId { get; set; }
        public string? RecipeCategoryGroupName { get; set; }
        public string? RecipeCategoryGroupLangCode { get; set; }
        public int? RecipeCategoryGroupType { get; set; }
    }
}
