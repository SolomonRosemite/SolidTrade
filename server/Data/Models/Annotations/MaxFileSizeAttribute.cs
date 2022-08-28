using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SolidTradeServer.Data.Models.Annotations
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult IsValid(
            object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult(GetErrorMessage(file.Length));
                }
            }

            return ValidationResult.Success;
        }

        private string GetErrorMessage(long fileSize)
            => $"Maximum allowed file size is {_maxFileSize / 1000000} megabytes. But uploaded file size was {((float) fileSize / 1000000):0.00} megabytes.";
    }
}