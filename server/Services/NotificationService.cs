using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Serilog;

namespace SolidTradeServer.Services
{
    public class NotificationService
    {
        private readonly ILogger _logger = Log.ForContext<NotificationService>();
        private readonly FirebaseMessaging _fireMessaging = FirebaseMessaging.DefaultInstance;

        public Task SendNotification(int userId, string registrationToken, string title, string message)
        {
            var data = new Dictionary<string, string>
            {
                { "Title", title },
                { "Body", message },
            };
            
            _logger.Information("Send notification to user with id {@UserId} with message {@Data}", userId, data);
            
            // Todo: See if metadata can also be send and received by the client.
            // If so, using firestore to notify the client about a ongoing position being filled will not be necessary.
            // We will use this notification message instead.

            return Task.CompletedTask;
            // Todo: Fix exception.
            // return _fireMessaging.SendAsync(new Message
            // {
            //     Token = registrationToken,
            //     Data = data,
            //     Android =
            //     {
            //         Priority = Priority.Normal,
            //     },
            // });
        }
    }
}