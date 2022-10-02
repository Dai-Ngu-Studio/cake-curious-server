using BusinessObject;
using Repository.Constants.Reports;
using Repository.Models.Comments;
using Repository.Models.RecipeMaterials;
using Repository.Models.RecipeMedia;
using Repository.Models.RecipeSteps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Product
{
    public class ItemReportContent
    {   
        public Guid Id { get; set; }
        public string? ItemType { get; set; }
        public IEnumerable<SimpleRecipeMedia>? VideoLinks  { get; set; }
        public IEnumerable<SimpleRecipeMedia>? ImageLinks { get; set; }
        public IEnumerable<SimpleRecipeStep>? Steps { get; set; }
        public IEnumerable<SimpleRecipeMaterial>? RecipeMerterials { get; set; }
        public string? Desciption { get; set; }  
        public string? commentContent { get; set; }
        public IEnumerable<RecipeCommentMedia>? commentMedia { get; set; }
    }
}
