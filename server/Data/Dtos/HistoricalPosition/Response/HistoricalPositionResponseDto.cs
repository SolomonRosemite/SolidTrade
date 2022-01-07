using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Dtos.HistoricalPosition.Response
{
    public class HistoricalPositionResponseDto : BaseEntity
    {
        public PositionType PositionType { get; set; }
        
        public BuyOrSell BuyOrSell { get; set; }
        
        public decimal BuyInPrice { get; set; }
        
        public decimal Performance { get; set; }
        
        public int NumberOfShares { get; set; }
        
        public string Isin { get; set; }
    }
}