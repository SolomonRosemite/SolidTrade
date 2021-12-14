using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Entities
{
    public class OngoingWarrantPosition : BaseEntity
    {
        public WarrantDerivative WarrantDerivative { get; set; }
        public EnterOrExitPositionType Type { get; set; }
        public Portfolio Portfolio { get; set; }
        public WarrantPosition CurrentWarrantPosition { get; set; }
        public float Price { get; set; }
    }
}