using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Entities
{
    public class OngoingKnockoutPosition : BaseEntity
    {
        public KnockoutDerivative KnockoutDerivative { get; set; }
        public EnterOrExitPositionType Type { get; set; }
        public Portfolio Portfolio { get; set; }
        public KnockoutPosition CurrentKnockoutPosition { get; set; }
        public float Price { get; set; }
    }
}