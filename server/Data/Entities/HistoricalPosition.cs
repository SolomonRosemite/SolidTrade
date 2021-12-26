using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Entities
{
    public class HistoricalPosition : BaseEntity
    {
        public PositionType PositionType { get; set; }
        public BuyOrSell BuyOrSell { get; set; }
        public float BuyInPrice { get; set; }
        public float Performance { get; set; }
        public int NumberOfShares { get; set; }
        public string Isin { get; set; }
    }
}