using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using Serilog;
using SolidTradeServer.Common;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;

namespace SolidTradeServer.Services.Common
{
    public static class CommonService
    {
        public static FirestoreDb Firestore { get; set; }
        private static readonly ILogger _logger = Log.Logger;

        public static IActionResult MatchResult<T>(OneOf<T, ErrorResponse> value)
        {
            return value.Match(
                response => new ObjectResult(response),
                err =>
                {
                    _logger.Error(Constants.LogMessageTemplate, err.Error);
                    
                    return new ObjectResult(new UnexpectedError
                    {
                        Title = err.Error.Title,
                        UserFriendlyMessage = err.Error.UserFriendlyMessage,
                        Message = err.Error.Message,
                    }) {StatusCode = (int) err.Code};
                });
        }
    }
}
