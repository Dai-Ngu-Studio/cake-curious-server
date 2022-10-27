using BusinessObject;
using Repository.Models.RecipeCategories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.RecipeHasCategories
{
    public class SimpleRecipeHasCategory
    {
        public Guid Id { get; set; }
        public int? RecipeCategoryId { get; set; }
        public DetachedRecipeCategory? RecipeCategory { get; set; }

    }
}
