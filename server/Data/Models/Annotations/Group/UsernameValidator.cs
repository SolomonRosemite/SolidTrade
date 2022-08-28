using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using SolidTradeServer.Common;

namespace SolidTradeServer.Data.Models.Annotations.Group
{
    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UsernameValidator : ValidationAttribute
    {
        private readonly ValidationAttribute[] _attributes;

        public UsernameValidator()
        {
            _attributes = new ValidationAttribute[]
            {
                new RegularExpressionAttribute("^(?!.*\\.\\.)(?!.*\\.$)[^\\W][\\w.]{3,29}$") { ErrorMessage = ErrorMessages.UsernameFormatMessage }
            };
        }
        
        public override bool IsValid(object value)
        {
            return _attributes.All(a => a.IsValid(value));
        }
    }
}