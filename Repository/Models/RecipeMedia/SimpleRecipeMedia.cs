using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.RecipeMedia
{
    public class SimpleRecipeMedia
    {
        public Guid Id { get; set; }
        public string? MediaUrl { get; set; }
        public int? MediaType { get; set; }
    }
}
