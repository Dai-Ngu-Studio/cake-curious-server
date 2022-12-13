using System.ComponentModel.DataAnnotations;

namespace Repository.Models.Notifications
{
    public class NotificationIds
    {
        [Required]
        public IEnumerable<Guid>? Ids { get; set; }
    }
}
