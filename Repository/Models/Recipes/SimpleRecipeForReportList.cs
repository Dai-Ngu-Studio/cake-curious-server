using BusinessObject;
using Repository.Models.RecipeHasCategories;
using Repository.Models.RecipeMaterials;
using Repository.Models.RecipeMedia;
using Repository.Models.RecipeSteps;
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

        public int? TotalPendingReports { get; set; }

        public ICollection<SimpleRecipeHasCategory>? HasCategories { get; set; }

        public ICollection<SimpleRecipeMaterial>? RecipeMaterials { get; set; }

        public ICollection<SimpleRecipeMedia>? RecipeMedia { get; set; }

        public ICollection<SimpleRecipeStep>? RecipeSteps { get; set; }
    }
}
