using System.ComponentModel.DataAnnotations;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Dtos.ProductImage.Request
{
    public class GetProductImageRequestDto
    {
        [Required] 
        public string Isin { get; set; }

        [Required] 
        public ProductImageThemeColor? ThemeColor { get; set; }

        [Required]
        public bool IsWeb { get; set; }
    }
}