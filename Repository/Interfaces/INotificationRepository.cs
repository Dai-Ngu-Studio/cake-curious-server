﻿using BusinessObject;
using Repository.Models.Notifications;

namespace Repository.Interfaces
{
    public interface INotificationRepository
    {
        public Task CreateNotificationContent(NotificationContent content);
        public Task CreateNotifications(List<Notification> notifications);
        public Task<int> CountUnreadOfUser(string uid);
        public IEnumerable<DetailNotifcation> GetNotificationsOfUser(string uid, int skip, int take);
        public Task SwitchNotificationStatus(Guid id);
        public Task MarkAsRead(Guid id);
        public Task MarkAllAsRead(string userId);
        public Task UpdateRangeNotificationStatus(List<Guid> ids, int status);
    }
}
