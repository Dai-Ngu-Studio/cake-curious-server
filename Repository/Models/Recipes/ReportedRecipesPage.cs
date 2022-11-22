using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Recipes
{
    public class ReportedRecipesPage
    {
        public IEnumerable<SimpleRecipeForReportList>? Recipes { get; set; }
        public int? TotalPage { get; set; }

    }
}
