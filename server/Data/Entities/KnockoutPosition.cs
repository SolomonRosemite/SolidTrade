using System.ComponentModel.DataAnnotations;
using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models.Common.Position;
using static SolidTradeServer.Common.Constants;

namespace SolidTradeServer.Data.Entities
{
    public class KnockoutPosition : BaseEntity, IPosition
    {
        [Required]
        public Portfolio Portfolio { get; set; }

        [Required]
        [MaxLength(12)]
        public string Isin { get; set; }
        
        [Required]
        [Range(MinimumNumberOfShares, int.MaxValue)]
        public decimal NumberOfShares { get; set; }
        
        [Required]
        public decimal BuyInPrice { get; set; }
    }
}