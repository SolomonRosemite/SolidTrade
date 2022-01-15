using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Dtos.Stock.Response
{
    public class StockPositionResponseDto : BaseEntity
    {
        public string Isin { get; set; }
        
        public decimal BuyInPrice { get; set; }
        public int NumberOfShares { get; set; }
    }
}