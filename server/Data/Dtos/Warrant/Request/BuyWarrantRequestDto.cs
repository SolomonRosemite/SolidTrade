using System.ComponentModel.DataAnnotations;

namespace SolidTradeServer.Data.Dtos.Warrant.Request
{
    public class BuyWarrantRequestDto
    {
        [Required]
        public string WarrantIsin { get; init; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int NumberOfShares { get; init; }
    }
}