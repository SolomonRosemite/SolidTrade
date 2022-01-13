namespace SolidTradeServer.Data.Dtos.Common
{
    public class TradeRepublicIsStockMarketOpenResponseDto
    {
        public long ExpectedCloseTime { get; init; }
        public bool? Open { get; init; }
    }
}