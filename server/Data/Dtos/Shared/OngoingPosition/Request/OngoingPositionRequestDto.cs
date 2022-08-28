using System;
using System.ComponentModel.DataAnnotations;
using SolidTradeServer.Data.Models.Annotations;
using SolidTradeServer.Data.Models.Enums;
using static SolidTradeServer.Common.Constants;
using static SolidTradeServer.Common.ErrorMessages;

namespace SolidTradeServer.Data.Dtos.Shared.OngoingPosition.Request
{
    public class OngoingPositionRequestDto
    {
        [Required] 
        public string Isin { get; set; }

        [Required]
        [Range(0.00010, int.MaxValue, ErrorMessage = PriceThresholdMessage)]
        public decimal PriceThreshold { get; set; }

        [Required]
        public EnterOrExitPositionType? Type { get; set; }
        
        [Required]
        [IsFutureDate(ErrorMessage = GoodUntilErrorMessage)]
        public DateTimeOffset? GoodUntil { get; set; }
        
        [Range(MinimumNumberOfShares, int.MaxValue)]
        public decimal NumberOfShares { get; set; }
    }
}