using System;
using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Dtos.OngoingKnockout.Response
{
    public class OngoingKnockoutPositionResponseDto : BaseEntity
    {
        public DateTimeOffset GoodUntil { get; set; }
        
        public string Isin { get; set; }
        
        public EnterOrExitPositionType Type { get; set; }
        
        public OngoingKnockoutPositionResponseDto CurrentKnockoutPosition { get; set; }
        public decimal Price { get; set; }
        public int NumberOfShares { get; set; }
    }
}