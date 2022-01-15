namespace SolidTradeServer.Data.Dtos.TradeRepublic
{
    public class TradeRepublicProductPriceResponseDto
    {
        public TradeRepublicProductPriceValueEntry Bid { get; init; }
        public TradeRepublicProductPriceValueEntry Ask { get; init; }
    }
    
    public class TradeRepublicProductPriceValueEntry
    {
        public long Time { get; init; }
        public decimal Price { get; init; }
    }
}