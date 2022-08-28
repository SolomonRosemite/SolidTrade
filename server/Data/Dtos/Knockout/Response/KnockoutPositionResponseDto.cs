using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Dtos.Knockout.Response
{
    public class KnockoutPositionResponseDto : BaseEntity
    {
        public string Isin { get; set; }
        
        public decimal NumberOfShares { get; set; }
        public decimal BuyInPrice { get; set; }
    }
}