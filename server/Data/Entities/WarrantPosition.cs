using System.ComponentModel.DataAnnotations;
using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models.Common.Position;

namespace SolidTradeServer.Data.Entities
{
    public class WarrantPosition : BaseEntity, IPosition
    {
        [Required]
        public Portfolio Portfolio { get; set; }
        
        [Required]
        [MaxLength(12)]
        public string Isin { get; set; }
        
        public decimal BuyInPrice { get; set; }
        public int NumberOfShares { get; set; }
    }
}