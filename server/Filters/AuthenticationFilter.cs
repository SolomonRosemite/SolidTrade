using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Data.Models.Errors.Common;

namespace SolidTradeServer.Filters
{
    public class AuthenticationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;
            var path = request.Path.Value?.ToLower();

            if (path is "/" or "/healthcheck" or null)
            {
                await next();
                return;
            }
            
            var token = request.Headers["Authorization"];

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedObjectResult(new NotAuthenticated
                {
                    Title = "Authorization header missing",
                    Message = "The authorization header was not specified.",
                    UserFriendlyMessage = "Login failed. Please try again",
                });
                return;
            }

            // Todo: Validate token with the Firebase sdk. 

            await next();
        }
    }
}