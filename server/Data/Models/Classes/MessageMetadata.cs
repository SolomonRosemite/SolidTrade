using System.ComponentModel.DataAnnotations;

namespace SolidTradeServer.Data.Models.Classes
{
    public class MessageMetadata
    {
        [Required]
        public string Token { get; init; }
    }
}