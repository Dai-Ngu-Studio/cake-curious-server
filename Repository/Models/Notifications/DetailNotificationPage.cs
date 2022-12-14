namespace Repository.Models.Notifications
{
    public class DetailNotificationPage
    {
        public int? UnreadCount { get; set; }
        public IEnumerable<DetailNotifcation>? Notifications { get; set; }
    }
}
