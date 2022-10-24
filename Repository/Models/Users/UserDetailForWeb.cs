﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Users
{
    public class UserDetailForWeb
    {
        public string? Id { get; set; } = null!;
   
        public string? Email { get; set; }
    
        public string? DisplayName { get; set; }
       
        public string? PhotoUrl { get; set; }
        
        public string? Gender { get; set; } 

        public List<string>? Roles { get; set; }
    }
}
