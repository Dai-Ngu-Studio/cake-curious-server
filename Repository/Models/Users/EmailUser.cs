using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Users
{
    public class EmailUser
    {
        [Required]
        public string? Email { get; set; }
    }
}
