using System;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Configuration;
using OneOf;

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
                return (true, _configuration["TestUserUid"]);

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