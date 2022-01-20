using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SolidTradeServer.Data.Entities.Common;

namespace SolidTradeServer.Data.Entities
{
    public class User : BaseEntity
    {
        public Portfolio Portfolio { get; set; }
        public ICollection<HistoricalPosition> HistoricalPositions { get; set; }
        
        [Required]
        [MaxLength(512)]
        public string Bio { get; set; }
        
        [Required]
        [MaxLength(32)]
        public string Username { get; set; }
                
        [Required]
        [MaxLength(32)]
        public string DisplayName { get; set; }
                
        [MaxLength(255)]
        public string ProfilePictureUrl { get; set; }
                
        [Required]
        [MaxLength(64)]
        public string Email { get; set; }
                
        [Required]
        [MaxLength(128)]
        public string Uid { get; set; }
        
        public bool HasPublicPortfolio { get; set; }
    }
}