using CakeCurious_API.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Constants.NotificationContents;
using Repository.Interfaces;
using Repository.Models.Notifications;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository notificationRepository;
        private readonly IStoreRepository storeRepository;
        private readonly IUserRepository userRepository;
        private readonly IUserDeviceRepository userDeviceRepository;

        public NotificationsController(INotificationRepository _notificationRepository, IStoreRepository _storeRepository,
            IUserRepository _userRepository, IUserDeviceRepository _userDeviceRepository)
        {
            notificationRepository = _notificationRepository;
            storeRepository = _storeRepository;
            userRepository = _userRepository;
            userDeviceRepository = _userDeviceRepository;
        }

        [HttpPut("{id:guid}/read")]
        [Authorize]
        public async Task<ActionResult> MarkAsRead(Guid id)
        {
            await notificationRepository.SwitchNotificationStatus(id);
            return Ok();
        }

        [HttpPut("read-selected")]
        [Authorize]
        public ActionResult MarkSelectedAsRead(NotificationIds notifications)
        {
            if (notifications.Ids!.Count() > 0)
            {
                return StatusCode(501);
            }
            return StatusCode(501);
        }

        [HttpPost("push")]
        [Authorize]
        public async Task<ActionResult> PushNotification(PushNotification notification)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var isSenderExisted = await userRepository.IsUserExisted(uid);
                switch (notification.ItemType)
                {
                    case (int)NotificationContentItemTypeEnum.StoreChat:
                        if (isSenderExisted)
                        {
                            // Get store ID by UID
                            var storeId = await storeRepository.getStoreIdByUid(uid);
                            // Check if sender have store
                            if (storeId != Guid.Empty)
                            {
                                await NotificationUtility
                                    .NotifyChatToBaker(userDeviceRepository, notification, storeId);
                                return Ok();
                            }
                        }
                        return BadRequest();
                    case (int)NotificationContentItemTypeEnum.BakerChat:
                        return StatusCode(501);
                    case (int)NotificationContentItemTypeEnum.Generic:
                        // generic push notification
                        return StatusCode(501);
                    default:
                        return BadRequest();
                }
            }
            return BadRequest();
        }
    }
}
