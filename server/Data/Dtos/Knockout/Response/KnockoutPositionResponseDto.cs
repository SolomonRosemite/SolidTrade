using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Dtos.Knockout.Response
{
    public class KnockoutPositionResponseDto : BaseEntity
    {
        public string Isin { get; set; }
        
        public int NumberOfShares { get; set; }
        public float BuyInPrice { get; set; }
    }
}