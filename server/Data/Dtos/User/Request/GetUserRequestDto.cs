using SolidTradeServer.Data.Models.Annotations;

namespace SolidTradeServer.Data.Dtos.User.Request
{
    public class GetUserRequestDto
    {
        [RequiredIf(nameof(Uid), null)]
        public string Username { get; init; }
        
        [RequiredIf(nameof(Username), null)]
        public string Uid { get; init; }
    }
}