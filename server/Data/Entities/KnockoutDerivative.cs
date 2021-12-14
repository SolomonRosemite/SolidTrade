using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Entities
{
    public class KnockoutDerivative : BaseEntity
    {
        public AssetInfoSocieteGenerale AssetInfo { get; set; }
        public ICollection<KnockoutPosition> KnockoutPositions { get; set; }
        public LongOrShort LongOrShort { get; set; }
        
        [Column(TypeName = "char")]
        [StringLength(6)]
        public string Code { get; set; }
        public float StrikePrice { get; set; }
    }
}