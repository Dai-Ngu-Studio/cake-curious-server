namespace Repository.Models.NotificationContents
{
    public class DetailNotificationContent
    {
        public Guid? Id { get; set; }
        public Guid? ItemId { get; set; }
        public int? ItemType { get; set; }
        public string? ItemName { get; set; }
        public int? NotificationType { get; set; }
        public DateTime? NotificationDate { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
    }
}
