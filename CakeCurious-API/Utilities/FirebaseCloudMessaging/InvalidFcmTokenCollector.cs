using FirebaseAdmin.Messaging;
using Repository.Interfaces;

namespace CakeCurious_API.Utilities.FirebaseCloudMessaging
{
    public static class InvalidFcmTokenCollector
    {
        /// <summary>
        /// This method should be used after invoking SendMessagesAsync of FirebaseCloudMessageSender to delete any invalid tokens.
        /// </summary>
        /// <param name="response">The BatchReponse returned from SendMessagesAsync.</param>
        /// <param name="messages">List of Messages which were used as parameter in SendMessagesAsync.</param>
        /// <param name="userDeviceRepository">User device repository.</param>
        /// <returns></returns>
        public static async Task HandleBatchResponse(BatchResponse response, List<Message> messages, IUserDeviceRepository userDeviceRepository)
        {
            if (response.FailureCount > 0)
            {
                var failedTokens = new List<string>();
                for (var i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        failedTokens.Add(messages[i].Token);
                    }
                }
                await userDeviceRepository.RemoveRange(failedTokens);
            }
        }

        /// <summary>
        /// This method should be used after invoking SendMulticastAsync of FirebaseCloudMessageSender to delete any invalid tokens.
        /// </summary>
        /// <param name="response">The BatchReponse returned from SendMulticastAsync.</param>
        /// <param name="tokens">Tokens which were used as parameter in SendMulticastAsync.</param>
        /// <param name="userDeviceRepository">User device repository.</param>
        /// <returns></returns>
        public static async Task HandleMulticastBatchResponse(BatchResponse response, List<string> tokens, IUserDeviceRepository userDeviceRepository)
        {
            if (response.FailureCount > 0)
            {
                var failedTokens = new List<string>();
                for (var i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        failedTokens.Add(tokens[i]);
                    }
                }
                await userDeviceRepository.RemoveRange(failedTokens);
            }
        }
    }
}
