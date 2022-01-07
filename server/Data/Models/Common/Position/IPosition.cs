namespace SolidTradeServer.Data.Models.Common.Position
{
    public interface IPosition
    {
        public decimal BuyInPrice { get; set; }
        public int NumberOfShares { get; set; }
    }
}