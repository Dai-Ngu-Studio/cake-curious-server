using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Users
{
    public class UpdateProfileUser
    {
        [StringLength(128, ErrorMessage = "{0} must have {2}-{1} characters.", MinimumLength = 2)]
        public string? Username { get; set; }

        [StringLength(64, ErrorMessage = "{0} must have {2}-{1} characters.", MinimumLength = 2)]
        public string? DisplayName { get; set; }
    }
}
