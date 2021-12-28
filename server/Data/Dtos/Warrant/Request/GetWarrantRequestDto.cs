using System.ComponentModel.DataAnnotations;

namespace SolidTradeServer.Data.Dtos.Warrant.Request
{
    public class GetWarrantRequestDto
    {
        [Required]
        public string UnderlyingIsin { get; init; }
    }
}