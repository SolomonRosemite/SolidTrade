using SolidTradeServer.Data.Dtos.Knockout.Response;
using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Dtos.OngoingWarrant.Response
{
    public class OngoingWarrantPositionResponseDto : BaseEntity
    {
        public string Isin { get; set; }
        
        public EnterOrExitPositionType Type { get; set; }
        
        public KnockoutPositionResponseDto CurrentKnockoutPosition { get; set; }
        public float Price { get; set; }
    }
}