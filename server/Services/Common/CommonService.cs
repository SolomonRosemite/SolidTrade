using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using Serilog;
using Serilog.Core;
using SolidTradeServer.Data.Models.Common.Position;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;
using Constants = SolidTradeServer.Common.Constants;

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
                    var exception = err.Error.Exception;
                    err.Error.Exception = null;
                    _logger.Error(Constants.LogMessageTemplate, err.Error, exception);
                    
                    return new ObjectResult(new UnexpectedError
                    {
                        Title = err.Error.Title,
                        UserFriendlyMessage = err.Error.UserFriendlyMessage,
                        Message = err.Error.Message,
                    }) {StatusCode = (int) err.Code};
                });
        }

        public static IPosition CalculateNewPosition(IPosition p1, IPosition p2)
        {
            Position position = new Position
            {
                NumberOfShares = p1.NumberOfShares + p2.NumberOfShares,
            };

            position.BuyInPrice =
                (p1.BuyInPrice * p1.NumberOfShares +
                 p2.BuyInPrice * p2.NumberOfShares) / position.NumberOfShares;

            return position;
        }
        
        public static T ToObject<T>(this IDictionary<string, object> source)
            where T : class, new()
        {
            var someObject = new T();
            var someObjectType = someObject.GetType();

            foreach (var item in source)
            {
                someObjectType.GetProperty(item.Key)
                    .SetValue(someObject, item.Value, null);
            }

            return someObject;
        }

        public static IDictionary<string, object> AsDictionary(this object source, BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
        }
    }
}
