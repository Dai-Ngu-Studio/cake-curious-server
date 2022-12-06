using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Users
{
    public class CreateStaffUser
    {
        [Required]
        public string? Email { get; set; }
    }
}
