using System.ComponentModel.DataAnnotations;
using SolidTradeServer.Data.Entities.Common;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Entities
{
    public class ProductImageRelation : BaseEntity
    {
        [Required]
        [MaxLength(12)]
        public string Isin { get; set; }
        
        [Required]
        [MaxLength(512)]
        public string CorrespondingImageUrl { get; set; }
        
        [Required]
        public ProductImageThemeColor ThemeColor { get; set; }
    }
}