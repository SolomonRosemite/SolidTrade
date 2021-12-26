using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Entities
{
    public class OngoingWarrantPosition : BaseEntity
    {
        [Column(TypeName = "char")]
        [StringLength(12)]
        public string Isin { get; set; }
        
        public EnterOrExitPositionType Type { get; set; }
        public Portfolio Portfolio { get; set; }
        public WarrantPosition CurrentWarrantPosition { get; set; }
        public float Price { get; set; }
    }
}