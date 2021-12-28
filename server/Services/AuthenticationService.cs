using System.Threading.Tasks;
using SolidTradeServer.Data.Models.Classes;

namespace SolidTradeServer.Services
{
    public class AuthenticationService
    {
        public async Task<bool> AuthenticateUser(MessageMetadata metadata)
        {
            // Todo: Implement user authentication.
            return await Task.FromResult(true);
        }
    }
}