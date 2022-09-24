﻿using BusinessObject;
using Repository.Models.RecipeMaterials;
using Repository.Models.RecipeSteps;
using Repository.Models.Users;

namespace Repository.Models.Recipes
{
    public class DetailRecipe
    {
        public Guid? Id { get; set; }
        public SimpleUser? User { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? ServingSize { get; set; }
        public string? PhotoUrl { get; set; }
        public string? VideoUrl { get; set; }
        public decimal? CookTime { get; set; }
        public DateTime? PublishedDate { get; set; }
        public int? Likes { get; set; }
        public ICollection<DetachedRecipeMaterial>? Ingredients { get; set; }
        public ICollection<DetachedRecipeMaterial>? Equipment { get; set; }
        public ICollection<SimpleRecipeStep>? RecipeSteps { get; set; }
    }
}
