using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Entities
{
    public class OngoingWarrantPosition : BaseEntity
    {
        [Required]
        public Portfolio Portfolio { get; set; }
        
        [Required]
        public WarrantPosition CurrentWarrantPosition { get; set; }
        
        [Required]
        [MaxLength(12)]
        public string Isin { get; set; }
        
        [Required]
        public EnterOrExitPositionType Type { get; set; }
        
        public float Price { get; set; }
    }
}