using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models.Users
{
    public class UpdateUser
    {
        public string? Id { get; set; } = null!;
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
        public string? PhotoUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? CitizenshipNumber { get; set; }
        public DateTime? CitizenshipDate { get; set; }
        public int? Status { get; set; }
        public int? RoleId { get; set; }

    }
}
