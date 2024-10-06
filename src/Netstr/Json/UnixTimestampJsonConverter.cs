using System.Text.Json.Serialization;
using System.Text.Json;

namespace Netstr.Json
{
    /// <summary>
    /// Converts Unix time to DateTimeOffset.
    /// </summary>
    public class UnixTimestampJsonConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TryGetInt64(out var time))
            {
                return DateTimeOffset.FromUnixTimeSeconds(time);
            }

            return DateTimeOffset.MinValue;
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.ToUnixTimeSeconds());
        }
    }
}
