using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Roles
{
    public class SimpleHasRoles
    {
        public Guid? Id { get; set; }
 
        public int? RoleId { get; set; }

        public Role? Role { get; set; }
    }
}
