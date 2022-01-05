using SolidTradeServer.Data.Models.Annotations;

namespace SolidTradeServer.Data.Dtos.Portfolio.Request
{
    public class GetPortfolioRequestDto
    {
        [RequiredIf(nameof(PortfolioId), null)]
        public int? UserId { get; init; }
        
        [RequiredIf(nameof(UserId), null)]
        public int? PortfolioId { get; init; }

        public bool IncludeOngoingPositions { get; init; }
    }
}