using System;
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
            
            return _fireMessaging.SendMulticastAsync(new MulticastMessage
            {
                Tokens = new []{ registrationToken },
                Data = data,
            });
        }
    }
}