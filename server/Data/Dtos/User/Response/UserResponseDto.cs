using System.Collections.Generic;
using SolidTradeServer.Data.Dtos.HistoricalPosition.Response;
using SolidTradeServer.Data.Dtos.Portfolio.Response;
using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Dtos.User.Response
{
    public class UserResponseDto : BaseEntity
    {
        public PortfolioResponseDto Portfolio { get; set; }
        public IReadOnlyCollection<HistoricalPositionResponseDto> HistoricalPositions { get; set; }
        
        public string Username { get; set; }
                
        public string DisplayName { get; set; }
                
        public string ProfilePictureUrl { get; set; }
                
        public string Email { get; set; }
                
        public string Bio { get; set; }
        
        public string Uid { get; set; }
        
        public bool HasPublicPortfolio { get; set; }
    }
}