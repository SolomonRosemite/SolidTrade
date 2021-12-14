using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Entities
{
    public class WarrantPosition : BaseEntity
    {
        public Portfolio Portfolio { get; set; }
        public WarrantDerivative Warrant { get; set; }
        public float BuyInPrice { get; set; }
        public int NumberOfShares { get; set; }
    }
}