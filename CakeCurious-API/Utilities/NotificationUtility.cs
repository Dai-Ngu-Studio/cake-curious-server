using BusinessObject;
using CakeCurious_API.Utilities.FirebaseCloudMessaging;
using Repository.Constants.NotificationContents;
using Repository.Constants.Notifications;
using Repository.Constants.Orders;
using Repository.Interfaces;
using Repository.Models.Notifications;

namespace CakeCurious_API.Utilities
{
    public static class NotificationUtility
    {
        public static async Task NotifyOrderStatus(IUserDeviceRepository userDeviceRepository,
            INotificationRepository notificationRepository, Order order, string userId, string storeName)
        {
            try
            {
                var userDevices = userDeviceRepository.GetDevicesOfUserReadonly(userId);
                var tokens = userDevices.Select(x => x.Token).ToList();
                // Create notification content
                var notificationContent = new NotificationContent
                {
                    ItemId = order.Id,
                    ItemType = (int)NotificationContentItemTypeEnum.Order,
                    ItemName = storeName,
                    NotificationDate = DateTime.Now,
                    Title = NotificationText.OrderStatusTitle,
                    EnTitle = NotificationText.OrderStatusTitleEn,
                    Notifications = new HashSet<Notification>()
                };
                var prefix = $"{NotificationText.OrderForStorePrefix} {storeName}";
                switch (order.Status)
                {
                    case (int)OrderStatusEnum.Processing:
                        notificationContent.NotificationType = (int)NotificationContentTypeEnum.OrderProcessed;
                        notificationContent.Content = $"{prefix} {NotificationText.OrderProcessed}.";
                        notificationContent.EnContent = $"{prefix} {NotificationText.OrderProcessedEn}.";
                        break;
                    case (int)OrderStatusEnum.Completed:
                        notificationContent.NotificationType = (int)NotificationContentTypeEnum.OrderCompleted;
                        notificationContent.Content = $"{prefix} {NotificationText.OrderCompleted}.";
                        notificationContent.EnContent = $"{prefix} {NotificationText.OrderCompletedEn}.";
                        break;
                    case (int)OrderStatusEnum.Cancelled:
                        notificationContent.NotificationType = (int)NotificationContentTypeEnum.OrderCancelled;
                        notificationContent.Content = $"{prefix} {NotificationText.OrderCancelled}.";
                        notificationContent.EnContent = $"{prefix} {NotificationText.OrderCancelledEn}.";
                        break;
                    default:
                        break;
                }

                if (notificationContent.NotificationType != null)
                {
                    // Create notification target
                    var notificationTarget = new Notification
                    {
                        UserId = userId,
                        Status = (int)NotificationStatusEnum.Unread,
                    };
                    notificationContent.Notifications!.Add(notificationTarget);
                    // Save changes in database
                    await notificationRepository.CreateNotificationContent(notificationContent);
                    // Send cloud message
                    var multicastMessage = new FirebaseAdmin.Messaging.MulticastMessage
                    {
                        Tokens = tokens,
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = notificationContent.Title,
                            Body = notificationContent.Content,
                        },
                        Data = new Dictionary<string, string>
                        {
                            { "itemType", notificationContent.ItemType.ToString()! },
                            { "itemId", notificationContent.ItemId.ToString()! },
                            { "notificationType", notificationContent.NotificationType.ToString()! }
                        }
                    };
                    var response = await FirebaseCloudMessageSender.SendMulticastAsync(multicastMessage);
                    await InvalidFcmTokenCollector.HandleMulticastBatchResponse(response, tokens!, userDeviceRepository);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}\n{e.InnerException}\n{e.StackTrace}");
            }
        }

        public static async Task NotifyChatToBaker(IUserDeviceRepository userDeviceRepository,
            PushNotification notification, Guid storeId)
        {
            var userDevices = userDeviceRepository.GetDevicesOfUserReadonly(notification.ReceiverId!);
            var tokens = userDevices.Select(x => x.Token).ToList();
            var multicastMessage = new FirebaseAdmin.Messaging.MulticastMessage
            {
                Tokens = tokens,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = notification.Title,
                    Body = notification.Content,
                },
                Data = new Dictionary<string, string>
                {
                    { "itemType", notification.ItemType.ToString()! },
                    { "itemId", storeId.ToString() },
                    { "notificationType", ((int)NotificationContentTypeEnum.Chat).ToString() }
                }
            };
            var response = await FirebaseCloudMessageSender.SendMulticastAsync(multicastMessage);
            await InvalidFcmTokenCollector.HandleMulticastBatchResponse(response, tokens!, userDeviceRepository);
        }
    }
}
