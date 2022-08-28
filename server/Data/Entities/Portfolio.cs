using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Entities
{
    public class Portfolio : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        
        [Required]
        public decimal Cash { get; set; }
        
        [Required]
        public decimal InitialCash { get; set; }
        
        public ICollection<StockPosition> StockPositions { get; set; }
        public ICollection<WarrantPosition> WarrantPositions { get; set; }
        public ICollection<KnockoutPosition> KnockOutPositions { get; set; }
        public ICollection<OngoingWarrantPosition> OngoingWarrantPositions { get; set; }
        public ICollection<OngoingKnockoutPosition> OngoingKnockOutPositions { get; set; }
    }
}