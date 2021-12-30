using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace SolidTradeServer.Services.Common
{
    public static class CommonService
    {
        public static IActionResult MatchResult<T>(OneOf<T, ErrorResponseModel> value)
        {
            return value.Match(
                response => new ObjectResult(response),
                err => new ObjectResult(err.Error) {StatusCode = (int) err.Code});
        }
    }
}
