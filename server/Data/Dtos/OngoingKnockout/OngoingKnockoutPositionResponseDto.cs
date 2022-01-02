using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Dtos.OngoingKnockout
{
    public class OngoingKnockoutPositionResponseDto : BaseEntity
    {
        public string Isin { get; set; }
        
        public EnterOrExitPositionType Type { get; set; }
        
        public OngoingKnockoutPositionResponseDto CurrentKnockoutPosition { get; set; }
        public float Price { get; set; }
    }
}