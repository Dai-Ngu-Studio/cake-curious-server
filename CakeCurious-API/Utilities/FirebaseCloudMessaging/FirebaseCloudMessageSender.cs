using FirebaseAdmin.Messaging;

namespace CakeCurious_API.Utilities.FirebaseCloudMessaging
{
    public static class FirebaseCloudMessageSender
    {
        /// <summary>
        /// Sends a message to the FCM service for it to be delivered to the client app.
        /// </summary>
        /// <param name="message">Message must include a FcmToken.</param>
        public static async Task SendMessageAsync(Message message)
        {
            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }

        /// <summary>
        /// Sends all messages in the given list. Up to 500 messages can be batched.
        /// Messages are not required to have the same FcmToken, Notification or Data.
        /// For example, two different messages with two different FcmTokens can be in the same list.
        /// This is the preferred method for sending multiple messages efficiently.
        /// </summary>
        /// <param name="messages">Each Message must include a FcmToken.</param>
        public static async Task<BatchResponse> SendMessagesAsync(List<Message> messages)
        {
            return await FirebaseMessaging.DefaultInstance.SendAllAsync(messages);
        }

        /// <summary>
        /// Send a message to multiple devices. Up to 500 device tokens can be included.
        /// </summary>
        /// <param name="message">Tokens parameter must be included.</param>
        /// <returns></returns>
        public static async Task<BatchResponse> SendMulticastAsync(MulticastMessage message)
        {
            return await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
        }
    }
}
