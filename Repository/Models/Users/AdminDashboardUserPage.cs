using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Users
{
    public class AdminDashboardUserPage
    {
        public IEnumerable<AdminDashboardUser>? Users { get; set; }
        public int TotalPage { get; set; } 
    }
}
