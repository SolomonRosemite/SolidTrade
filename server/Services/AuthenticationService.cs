using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace SolidTradeServer.Services
{
    public class AuthenticationService
    {
        private readonly IConfiguration _configuration;

        public AuthenticationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<(bool, string)> AuthenticateUser(string token)
        {
            if (_configuration.GetValue<bool>("IsLocalDevelopment"))
            {
                // When we are testing the api we just provide the uid in the Authorization header to authenticate.
                try { await FirebaseAuth.DefaultInstance.GetUserAsync(token); }
                catch
                {
                    return (false, null);
                }
                return (true, token);
            }
            
            try
            {
                FirebaseToken decodedToken = await FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(token);
                return (true, decodedToken.Uid);
            }
            catch
            {
                return (false, null);
            } 
        }
    }
}