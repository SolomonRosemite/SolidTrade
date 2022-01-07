using System;
using System.ComponentModel.DataAnnotations;
using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Entities
{
    public class OngoingKnockoutPosition : BaseEntity
    {
        [Required] 
        public DateTimeOffset GoodUntil { get; set; }
        
        [Required]
        public Portfolio Portfolio { get; set; }
        
        [Required]
        [MaxLength(12)]
        public string Isin { get; set; }
        
        [Required]
        public EnterOrExitPositionType Type { get; set; }
        
        [Required]
        public KnockoutPosition CurrentKnockoutPosition { get; set; }
        public decimal Price { get; set; }
    }
}