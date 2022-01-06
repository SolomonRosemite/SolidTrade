using System;

namespace SolidTradeServer.Data.Dtos.Warrant.TradeRepublic
{
    public class TradeRepublicWarrantResponseDto
    {
        public TradeRepublicWarrantValueEntry Bid { get; init; }
        public TradeRepublicWarrantValueEntry Ask { get; init; }
    }
    
    public class TradeRepublicWarrantValueEntry
    {
        public long Time { get; init; }
        public decimal Price { get; init; }
        public decimal Size { get; init; }
    }
}