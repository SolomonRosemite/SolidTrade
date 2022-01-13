using System.ComponentModel.DataAnnotations;

namespace SolidTradeServer.Data.Dtos.Shared.OngoingPosition.Request
{
    public class CloseOngoingPositionRequestDto
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }
    }
}