using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Entities
{
    public class AssetInfoSocieteGenerale : BaseEntity
    {
        public int AssetId { get; set; }
        
        [Column(TypeName = "char")]
        [StringLength(3)]
        public string Currency { get; set; }
        
        [Column(TypeName = "char")]
        [StringLength(12)]
        public string Ticker { get; set; }
        
        [Column(TypeName = "char")]
        [StringLength(255)]
        public string AssetImageUrl { get; set; }
        
        [Column(TypeName = "char")]
        [StringLength(128)]
        public string Name { get; set; }
    }
}