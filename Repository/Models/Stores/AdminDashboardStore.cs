using Repository.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Stores
{
    public class AdminDashboardStore
    {
        public Guid? Id { get; set; }
        public int? Status { get; set; }
        public string? PhotoUrl { get; set; }
        public decimal? Rating { get; set; }
        public string? Address  { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public SimpleUser? User { get; set; }

    }
}
