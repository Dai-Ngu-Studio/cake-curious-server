using BusinessObject;
using Repository.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Recipes
{
    public class SimpleRecipeForReportList
    {
        public Guid? Id { get; set; }

        public string? UserId { get; set; }

        public StoreDetailUser? User { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public int? ServingSize { get; set; }

        public string? PhotoUrl { get; set; }

        public int? CookTime { get; set; }

        public DateTime? PublishedDate { get; set; }

        public int? Status { get; set; }

        public ICollection<RecipeHasCategory>? HasCategories { get; set; }

        public ICollection<RecipeMaterial>? RecipeMaterials { get; set; }

        public ICollection<BusinessObject.RecipeMedia>? RecipeMedia { get; set; }

        public ICollection<RecipeStep>? RecipeSteps { get; set; }
    }
}
