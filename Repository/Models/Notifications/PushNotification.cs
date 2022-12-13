using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Notifications
{
    public class PushNotification
    {
        [Required]
        public string? ReceiverId { get; set; }

        [Required]
        public int? ItemType { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }
    }
}
