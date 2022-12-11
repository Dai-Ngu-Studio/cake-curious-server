using BusinessObject;

namespace Repository.Interfaces
{
    public interface INotificationRepository
    {
        public Task CreateNotificationContent(NotificationContent content);
    }
}
