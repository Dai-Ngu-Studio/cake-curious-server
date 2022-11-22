using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Stores
{
    public class CreateStoreRequest
    {
        // User
        [Required]
        public string? FullName { get; set; }

        [Required]
        public string? Gender { get; set; }

        [Required]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        public string? Address { get; set; }

        [Required]
        public string? CitizenshipNumber { get; set; }

        [Required]
        public DateTime? CitizenshipDate { get; set; }

        // Store
        [Required]
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PhotoUrl { get; set; }

        [Required]
        public string? StoreAddress { get; set; }
    }
}
