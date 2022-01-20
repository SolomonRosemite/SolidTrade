using System;
using System.ComponentModel.DataAnnotations;
using SolidTradeServer.Data.Models.Annotations;
using SolidTradeServer.Data.Models.Enums;

namespace SolidTradeServer.Data.Dtos.Shared.OngoingPosition.Request
{
    public class OngoingPositionRequestDto
    {
        [Required] 
        public string Isin { get; set; }

        [Required]
        [Range(0.00010, int.MaxValue)]
        public decimal PriceThreshold { get; set; }

        [Required]
        public EnterOrExitPositionType? Type { get; set; }
        
        [Required]
        [IsFutureDate(ErrorMessage = "Good Until must be future date.")]
        public DateTimeOffset? GoodUntil { get; set; }
        
        [Range(1, int.MaxValue)]
        public int NumberOfShares { get; set; }
    }
}