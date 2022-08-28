using System;
using SolidTradeServer.Data.Dtos.Warrant.Response;
using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Dtos.OngoingWarrant.Response
{
    public class OngoingWarrantPositionResponseDto : BaseEntity
    {
        public DateTimeOffset GoodUntil { get; set; }
        
        public string Isin { get; set; }
        
        public EnterOrExitPositionType Type { get; set; }
        
        public WarrantPositionResponseDto CurrentWarrantPosition { get; set; }
        public decimal Price { get; set; }
        public decimal NumberOfShares { get; set; }
    }
}