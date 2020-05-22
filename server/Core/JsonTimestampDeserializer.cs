using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Google.Protobuf.WellKnownTypes;


namespace UST.Server
{
    public class JsonTimestampDeserializer : JsonConverter<Timestamp>
    {       
        public override Timestamp Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options) =>
            DateTime.SpecifyKind(DateTime.Parse(reader.GetString()), DateTimeKind.Utc).ToTimestamp();

        public override void Write(Utf8JsonWriter writer, Timestamp timestampValue, JsonSerializerOptions options) =>
            writer.WriteStringValue(timestampValue.ToString());
    }
}
