using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SolidTradeServer.Data.Models.Converters
{
    public class StringRemoveWhitespaceConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
          => reader.GetString()?.Trim();

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            var item = options.Converters
                .First(converter => converter.GetType() == typeof(StringRemoveWhitespaceConverter));

            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Remove(item);
            
            JsonSerializer.Serialize(writer, value, newOptions);
        }
    }
}