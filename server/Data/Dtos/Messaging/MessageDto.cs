using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using SolidTradeServer.Data.Models.Annotations;
using SolidTradeServer.Data.Models.Classes;
using SolidTradeServer.Data.Models.Enums;
using SolidTradeServer.Data.Models.Errors;

namespace SolidTradeServer.Data.Dtos.Messaging
{
    public class MessageDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The field {0} must be greater than {1}.")]
        public int Id { get; init; }
        
        [Required, ValidateObject]
        public MessageMetadata Metadata { get; init; }

        [Required]
        [Range(1, byte.MaxValue, ErrorMessage = "The {0} field is required.")]
        public MessageType MessageType { get; init; }

        [Required]
        public object Data { get; init; }
        
        public OneOf<T, InvalidJsonFormat> CastTo<T>(bool runValidation = true)
        {
            return ConvertToObject<T>(Data).Match(
                deserializeObject =>
                runValidation ? ValidateObject(deserializeObject) : deserializeObject,
                format => format);
        }
        
        public static OneOf<MessageDto, InvalidJsonFormat> ToMessage(byte[] content)
        {
            return ConvertToObject<MessageDto>(Encoding.UTF8.GetString(content)).Match(
                ValidateObject, format => format);
        }

        private static OneOf<T, InvalidJsonFormat> ConvertToObject<T>(object data)
        {
            if (data is null)
            {
                return new InvalidJsonFormat {Title = "Json object was null"};
            }

            if (data is JObject obj)
                return obj.ToObject<T>();
            
            try
            {
                return JsonConvert.DeserializeObject<T>(data as string);
            }
            catch (Exception e)
            {
                return new InvalidJsonFormat
                {
                    Title = "Json format invalid", Message = "Unable to process invalid json.", Exception = e
                };
            }
        }
        
        private static OneOf<T, InvalidJsonFormat> ValidateObject<T>(T deserializeObject)
        {
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(deserializeObject, new ValidationContext(deserializeObject),
                validationResults, true);

            if (!isValid)
            {
                return validationResults
                    .Select(error => new InvalidJsonFormat
                    {
                        Title = "Validation error", Message = error.ErrorMessage
                    }).First();
            }

            return deserializeObject;
        }
    }
}