using System;
using System.ComponentModel.DataAnnotations;

namespace SolidTradeServer.Data.Models.Annotations
{
    public class IsFutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var now = DateTimeOffset.UtcNow;
            var min = now.AddHours(24 - now.Hour);

            if (value is null)
                return false;
            
            var dt = (DateTimeOffset) value;
            
            return dt >= min;
        }
    }
}