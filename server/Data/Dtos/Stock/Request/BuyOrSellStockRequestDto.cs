using System.ComponentModel.DataAnnotations;

namespace SolidTradeServer.Data.Dtos.Stock.Request
{
    public class BuyOrSellStockRequestDto
    {
        [Required]
        public string StockIsin { get; init; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int NumberOfShares { get; init; }
    }
}