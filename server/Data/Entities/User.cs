using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Entities
{
    public class User : BaseEntity
    {
        public Portfolio Portfolio { get; set; }
        public HistoricalPosition HistoricalPosition { get; set; }
        
        [Column(TypeName = "char")]
        [StringLength(32)]
        public string Username { get; set; }
                
        [Column(TypeName = "char")]
        [StringLength(32)]
        public string DisplayName { get; set; }
                
        [Column(TypeName = "char")]
        [StringLength(255)]
        public string ProfilePictureUrl { get; set; }
                
        [Column(TypeName = "char")]
        [StringLength(64)]
        public string Email { get; set; }
        public bool HasPublicPortfolio { get; set; }
    }
}