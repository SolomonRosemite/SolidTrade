using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Entities
{
    public class KnockoutPosition : BaseEntity
    {
        public Portfolio Portfolio { get; set; }
        public KnockoutDerivative KnockoutDerivative { get; set; }
        public int NumberOfShares { get; set; }
        public float BuyInPrice { get; set; }
    }
}