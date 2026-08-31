using System.Text.Json;
using System.Text.Json.Serialization;

namespace BackEndWaterFloodApp.Converters
{
    // DateTime converter to handle non-nullable DateTime values
    public class DateTimeNonNullableConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        )
        {
            var value = reader.GetString();
            if (string.IsNullOrEmpty(value) || value == "0001-01-01T00:00:00")
                return DateTime.MinValue;

            return DateTime.Parse(value);
        }

        public override void Write(
            Utf8JsonWriter writer,
            DateTime value,
            JsonSerializerOptions options
        )
        {
            if (value == DateTime.MinValue)
            {
                writer.WriteNullValue();
            }
            else
            {
                writer.WriteStringValue(value.ToString("yyyy-MM-ddTHH:mm:ss"));
            }
        }
    }
}
