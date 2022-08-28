using System.ComponentModel.DataAnnotations;
using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models.Common.Position;
using SolidTradeServer.Data.Models.Enums;
using static SolidTradeServer.Common.Constants;

namespace SolidTradeServer.Data.Entities
{
    public class HistoricalPosition : BaseEntity, IPosition
    {
        public int UserId { get; set; }
        
        [Required]
        public PositionType PositionType { get; set; }
        
        [Required]
        public BuyOrSell BuyOrSell { get; set; }
        
        [Required]
        public decimal BuyInPrice { get; set; }
        
        [Required]
        public decimal Performance { get; set; }
        
        [Required]
        [Range(MinimumNumberOfShares, int.MaxValue)]
        public decimal NumberOfShares { get; set; }
        
        [Required]
        [MaxLength(12)]
        public string Isin { get; set; }
    }
}