using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Entities
{
    public class KnockoutPosition : BaseEntity
    {
        public Portfolio Portfolio { get; set; }
        
        [Column(TypeName = "char")]
        [StringLength(12)]
        public string Isin { get; set; }
        public int NumberOfShares { get; set; }
        public float BuyInPrice { get; set; }
    }
}