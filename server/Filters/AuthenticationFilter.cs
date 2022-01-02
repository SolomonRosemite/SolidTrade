using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SolidTradeServer.Data.Models.Errors;
using SolidTradeServer.Services;

namespace SolidTradeServer.Filters
{
    public class AuthenticationFilter : IAsyncActionFilter
    {
        private readonly AuthenticationService _authenticationService;

        public AuthenticationFilter(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

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

            var (successful, _) = await _authenticationService.AuthenticateUser(token);

            if (!successful)
            {
                context.Result = new UnauthorizedObjectResult(new NotAuthenticated
                {
                    Title = "Invalid token",
                    Message = "The token provided is expired or invalid.",
                    UserFriendlyMessage = "Login failed. Please try again",
                });
                return;
            }
            
            await next();
        }
    }
}