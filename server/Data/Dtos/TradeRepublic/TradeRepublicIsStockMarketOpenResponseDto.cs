namespace SolidTradeServer.Data.Dtos.TradeRepublic
{
    public class TradeRepublicIsStockMarketOpenResponseDto
    {
        public long? ExpectedClosingTime { get; init; }
        public long? Resolution { get; init; }
        public bool? Open { get; init; }
    }
}