using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.RecipeMaterials
{
    public class SimpleRecipeMaterial
    {
        public Guid? Id { get; set; }
        public string? MaterialName { get; set; }
        public decimal? Amount { get; set; }
        public string? Measurement { get; set; }
    }
}
