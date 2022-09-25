using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Product
{
    public class StoreDashboardProduct
    {
      
        public Guid? Id { get; set; }
    
        public string? Name { get; set; }
    
        public string? Description { get; set; }
        
        public int? Quantity { get; set; }

        public decimal? Price { get; set; }   
        
        public int? Status { get; set; }

        public string? PhotoUrl { get; set; }
    }
}
