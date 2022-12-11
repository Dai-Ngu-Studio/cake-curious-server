using BusinessObject;
using Repository.Models.Notifications;

namespace Repository.Interfaces
{
    public interface INotificationRepository
    {
        public Task CreateNotificationContent(NotificationContent content);
        public IEnumerable<DetailNotifcation> GetNotificationsOfUser(string uid, int skip, int take);
    }
}
