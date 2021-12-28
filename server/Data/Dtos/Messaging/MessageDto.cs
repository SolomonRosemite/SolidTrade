using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
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
        
        [Required]
        public MessageMetadata Metadata { get; init; }

        [Required]
        [Range(1, byte.MaxValue, ErrorMessage = "The {0} field is required.")]
        public MessageType MessageType { get; init; }

        [Required]
        public object Data { get; init; }
        
        public OneOf<T, IEnumerable<InvalidJsonFormat>> CastTo<T>(bool runValidation = true)
        {
            return ConvertToObject<T>(Data).Match(
                deserializeObject =>
                runValidation ? ValidateObject(deserializeObject) : deserializeObject,
                formats => formats.ToList());
        }
        
        public static OneOf<MessageDto, IEnumerable<InvalidJsonFormat>> ToMessage(byte[] content)
        {
            return ConvertToObject<MessageDto>(Encoding.UTF8.GetString(content)).Match(
                ValidateObject, formats => formats.ToList());
        }

        private static OneOf<T, IEnumerable<InvalidJsonFormat>> ConvertToObject<T>(object data)
        {
            if (data is null)
            {
                return new List<InvalidJsonFormat>
                {
                    new() { Title = "Json object was null" },
                };
            }

            if (data is JObject obj)
                return obj.ToObject<T>();
            
            try
            {
                return JsonConvert.DeserializeObject<T>(data as string);
            }
            catch (Exception e)
            {
                return new List<InvalidJsonFormat>
                {
                    new()
                    {
                        Title = "Json format invalid", Message = "Unable to process invalid json.", Exception = e
                    }
                };
            }
        }
        
        private static OneOf<T, IEnumerable<InvalidJsonFormat>> ValidateObject<T>(T deserializeObject)
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
                    }).ToList();
            }

            return deserializeObject;
        }
    }
}