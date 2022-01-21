using System.Collections.Generic;
using SolidTradeServer.Data.Dtos.Knockout.Response;
using SolidTradeServer.Data.Dtos.OngoingKnockout.Response;
using SolidTradeServer.Data.Dtos.OngoingWarrant.Response;
using SolidTradeServer.Data.Dtos.Stock.Response;
using SolidTradeServer.Data.Dtos.Warrant.Response;
using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Dtos.Portfolio.Response
{
    public class PortfolioResponseDto : BaseEntity
    {
        public int UserId { get; set; }
        
        public decimal Cash { get; set; }
        
        public decimal InitialCash { get; set; }
        
        public IReadOnlyCollection<StockPositionResponseDto> StockPositions { get; set; }
        public IReadOnlyCollection<WarrantPositionResponseDto> WarrantPositions { get; set; }
        public IReadOnlyCollection<KnockoutPositionResponseDto> KnockOutPositions { get; set; }
        public IReadOnlyCollection<OngoingWarrantPositionResponseDto> OngoingWarrantPositions { get; set; }
        public IReadOnlyCollection<OngoingKnockoutPositionResponseDto> OngoingKnockOutPositions { get; set; }
    }
}