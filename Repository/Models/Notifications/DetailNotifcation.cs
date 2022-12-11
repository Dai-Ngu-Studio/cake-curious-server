using Repository.Models.NotificationContents;

namespace Repository.Models.Notifications
{
    public class DetailNotifcation
    {
        public Guid? Id { get; set; }
        public Guid? ContentId { get; set; }
        public DetailNotificationContent? Content { get; set; }
        public int? Status { get; set; }
    }
}
