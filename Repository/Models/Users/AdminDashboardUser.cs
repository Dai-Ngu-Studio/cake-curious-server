using BusinessObject;
using Repository.Models.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Users
{
    public class AdminDashboardUser
    {    
        public string? Id { get; set; } = null!;     
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Gender { get; set; }
        public string? FullName { get; set; }
        public int? Status { get; set; }
        public List<string>? Roles { get; set; } = new List<string>();
    }
}
