using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Entities
{
    public class KnockoutPosition : BaseEntity
    {
        [Required]
        public Portfolio Portfolio { get; set; }

        [Required]
        [MaxLength(12)]
        public string Isin { get; set; }
        
        public int NumberOfShares { get; set; }
        public float BuyInPrice { get; set; }
    }
}