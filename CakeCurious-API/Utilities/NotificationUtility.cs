using BusinessObject;
using CakeCurious_API.Utilities.FirebaseCloudMessaging;
using Repository.Constants.NotificationContents;
using Repository.Constants.Notifications;
using Repository.Constants.Orders;
using Repository.Constants.Reports;
using Repository.Interfaces;
using Repository.Models.Notifications;
using System.Text.Json;

namespace CakeCurious_API.Utilities
{
    public static class NotificationUtility
    {
        public static async Task NotifyOrderStatus(IUserDeviceRepository userDeviceRepository,
            INotificationRepository notificationRepository, Order order, string userId, string storeName)
        {
            try
            {
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
                    var userDevices = userDeviceRepository.GetDevicesOfUserReadonly(userId);
                    var tokens = userDevices.Select(x => x.Token).ToList();
                    // Create notification target
                    var notificationTarget = new Notification
                    {
                        UserId = userId,
                        Status = (int)NotificationStatusEnum.Unread,
                    };
                    notificationContent.Notifications!.Add(notificationTarget);
                    // Save changes in database
                    await notificationRepository.CreateNotificationContent(notificationContent);

                    if (tokens.Count > 0)
                    {
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
                                { "notificationType", notificationContent.NotificationType.ToString()! },
                                { "notificationDate", JsonSerializer.Serialize(notificationContent.NotificationDate) }
                            }
                        };
                        var response = await FirebaseCloudMessageSender.SendMulticastAsync(multicastMessage);
                        await InvalidFcmTokenCollector.HandleMulticastBatchResponse(response, tokens!, userDeviceRepository);
                    }
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
            if (tokens.Count > 0)
            {
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
                        { "notificationType", ((int)NotificationContentTypeEnum.Chat).ToString() },
                        { "notificationDate", JsonSerializer.Serialize(DateTime.Now) }
                    }
                };
                var response = await FirebaseCloudMessageSender.SendMulticastAsync(multicastMessage);
                await InvalidFcmTokenCollector.HandleMulticastBatchResponse(response, tokens!, userDeviceRepository);
            }
        }

        public static async Task NotifyReporters(IUserDeviceRepository userDeviceRepository,
            INotificationRepository notificationRepository, IViolationReportRepository violationReportRepository,
            IRecipeRepository recipeRepository, ICommentRepository commentRepository, Guid itemId, int itemType)
        {
            try
            {
                var notificationContent = new NotificationContent
                {
                    ItemId = itemId,
                    Title = NotificationText.ReportProcessed,
                    EnTitle = NotificationText.ReportProcessedEn,
                    NotificationDate = DateTime.Now,
                };

                switch (itemType)
                {
                    case (int)ReportTypeEnum.Recipe:
                        var recipe = await recipeRepository.GetNameOnlyRecipeReadonly(itemId);
                        if (recipe != null)
                        {
                            notificationContent.ItemType = (int)NotificationContentItemTypeEnum.Recipe;
                            notificationContent.ItemName = recipe.Name;
                            notificationContent.NotificationType = (int)NotificationContentTypeEnum.ReportedRecipeTakenDown;
                            notificationContent.Content = $"{NotificationText.Recipe} {recipe.Name} {NotificationText.TakenDown}. {NotificationText.ThankYou}.";
                            notificationContent.EnContent = $"{NotificationText.RecipeEn} {recipe.Name} {NotificationText.TakenDownEn}. {NotificationText.ThankYouEn}.";
                            break;
                        }
                        return;
                    case (int)ReportTypeEnum.Comment:
                        var comment = await commentRepository.GetNameOnlyCommentReadonly(itemId);
                        if (comment != null)
                        {
                            notificationContent.ItemType = (int)NotificationContentItemTypeEnum.Comment;
                            notificationContent.ItemName = comment.UserDisplayName;
                            notificationContent.NotificationType = (int)NotificationContentTypeEnum.ReportedCommentTakenDown;
                            notificationContent.Content = $"{NotificationText.CommentOf} {comment.UserDisplayName} {NotificationText.TakenDown}. {NotificationText.ThankYou}.";
                            notificationContent.EnContent = $"{NotificationText.CommentOfEn} {comment.UserDisplayName} {NotificationText.TakenDownEn}. {NotificationText.ThankYouEn}.";
                            break;
                        }
                        return;
                    default:
                        return;
                }
                // Save changes in database
                await notificationRepository.CreateNotificationContent(notificationContent);

                var take = 20;
                var page = 0;
                var batches = new List<List<string>>();
                var initialBatch = violationReportRepository.GetReportersOfAnItemReadonly(itemId, page, take).ToList();
                batches.Add(initialBatch);
                while (batches.Count > 0)
                {
                    try
                    {
                        // Create notification targets
                        var notifications = batches.ElementAt(0)
                            .Select(x => new Notification
                            {
                                UserId = x,
                                Status = (int)NotificationStatusEnum.Unread,
                                ContentId = notificationContent.Id,
                            }).ToList();
                        // Save changes in database
                        await notificationRepository.CreateNotifications(notifications);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{e.Message}\n{e.InnerException}\n{e.StackTrace}");
                    }

                    // Get user devices from batch
                    var tokens = await userDeviceRepository.GetDeviceTokensOfUsersReadonly(batches.ElementAt(0));
                    try
                    {
                        if (tokens.Count > 0)
                        {
                            // Send multicast
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
                                    { "notificationType", notificationContent.NotificationType.ToString()! },
                                    { "notificationDate", JsonSerializer.Serialize(notificationContent.NotificationDate) }
                                }
                            };

                            var response = await FirebaseCloudMessageSender.SendMulticastAsync(multicastMessage);
                            await InvalidFcmTokenCollector.HandleMulticastBatchResponse(response, tokens, userDeviceRepository);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
                    }

                    var nextBatch = violationReportRepository.GetReportersOfAnItemReadonly(itemId, (++page * take), take).ToList();
                    batches.RemoveAt(0);
                    if (nextBatch != null && nextBatch.Any())
                    {
                        batches.Add(nextBatch);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}\n{e.InnerException}\n{e.StackTrace}");
            }
        }

        public static async Task NotifyReported(IUserDeviceRepository userDeviceRepository, IDeactivateReasonRepository deactivateReasonRepository,
            INotificationRepository notificationRepository, IRecipeRepository recipeRepository, ICommentRepository commentRepository,
            Guid itemId, int itemType)
        {
            try
            {
                var notificationContent = new NotificationContent
                {
                    ItemId = itemId,
                    Title = NotificationText.ReportedWarning,
                    EnTitle = NotificationText.ReportedWarningEn,
                    NotificationDate = DateTime.Now,
                };

                string userId = string.Empty;

                switch (itemType)
                {
                    case (int)ReportTypeEnum.Recipe:
                        var recipe = await recipeRepository.GetNameOnlyRecipeReadonly(itemId);
                        if (recipe != null)
                        {
                            userId = recipe.UserId ?? string.Empty;
                            notificationContent.ItemType = (int)NotificationContentItemTypeEnum.DeactivateReason;
                            notificationContent.ItemName = recipe.Name;
                            notificationContent.NotificationType = (int)NotificationContentTypeEnum.OwnRecipeTakenDown;
                            notificationContent.Content = $"{NotificationText.Recipe} {recipe.Name} {NotificationText.TakenDown} {NotificationText.ViewStandards}.";
                            notificationContent.EnContent = $"{NotificationText.RecipeEn} {recipe.Name} {NotificationText.TakenDownEn} {NotificationText.ViewStandardsEn}.";
                            break;
                        }
                        return;
                    case (int)ReportTypeEnum.Comment:
                        var comment = await commentRepository.GetNameOnlyCommentReadonly(itemId);
                        if (comment != null)
                        {
                            userId = comment.UserId ?? string.Empty;
                            notificationContent.ItemType = (int)NotificationContentItemTypeEnum.DeactivateReason;
                            notificationContent.ItemName = comment.UserDisplayName;
                            notificationContent.NotificationType = (int)NotificationContentTypeEnum.OwnCommentTakenDown;
                            notificationContent.Content = $"{NotificationText.CommentOf} {NotificationText.Your} {NotificationText.TakenDown} {NotificationText.ViewStandards}.";
                            notificationContent.EnContent = $"{NotificationText.CommentOfEn} {NotificationText.YourEn} {NotificationText.TakenDownEn} {NotificationText.ViewStandardsEn}.";
                            break;
                        }
                        return;
                    default:
                        return;
                }

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    // Create notification target
                    var notificationTarget = new Notification
                    {
                        UserId = userId,
                        Status = (int)NotificationStatusEnum.Unread,
                        ContentId = notificationContent.Id,
                    };

                    notificationContent.Notifications!.Add(notificationTarget);

                    // Save changes in database
                    await notificationRepository.CreateNotificationContent(notificationContent);

                    var userDevices = userDeviceRepository.GetDevicesOfUserReadonly(userId);
                    var tokens = userDevices.Select(x => x.Token).ToList();

                    if (tokens.Count > 0)
                    {
                        // Send multicast
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
                                    { "notificationType", notificationContent.NotificationType.ToString()! },
                                    { "notificationDate", JsonSerializer.Serialize(notificationContent.NotificationDate) }
                                }
                        };

                        var response = await FirebaseCloudMessageSender.SendMulticastAsync(multicastMessage);
                        await InvalidFcmTokenCollector.HandleMulticastBatchResponse(response, tokens!, userDeviceRepository);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}\n{e.InnerException}\n{e.StackTrace}");
            }
        }
    }
}
