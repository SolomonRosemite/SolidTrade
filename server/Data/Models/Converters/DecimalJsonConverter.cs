using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SolidTradeServer.Data.Models.Converters
{
    public class DecimalJsonConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetDecimal();

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            // We do this to insure that the decimal has at least one decimal place.
            // If there are no decimal places, dart will assume its a int which causes an exception while parsing the response body.
            // Also, using .toDouble() is not possible because the mappings are being generated and dont allow to add such an edge case to use the .toDouble() method
            // See here for more: https://github.com/google/googleapis.dart/issues/11#issuecomment-390766588
            writer.WriteNumberValue(value + 0.0m);
        }
    }
}