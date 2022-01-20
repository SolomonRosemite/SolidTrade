using System.ComponentModel.DataAnnotations;

namespace SolidTradeServer.Data.Dtos.Shared.Common
{
    public class BuyOrSellRequestDto
    {
        [Required]
        public string Isin { get; init; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int NumberOfShares { get; init; }
    }
}