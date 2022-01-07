using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Dtos.Warrant.Response
{
    public class WarrantPositionResponseDto : BaseEntity
    {
        public string Isin { get; set; }
        
        public decimal BuyInPrice { get; set; }
        public int NumberOfShares { get; set; }
    }
}