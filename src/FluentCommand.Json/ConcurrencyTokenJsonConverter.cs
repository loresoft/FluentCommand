using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluentCommand;

public class ConcurrencyTokenJsonConverter : JsonConverter<ConcurrencyToken>
{
    public override ConcurrencyToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new ConcurrencyToken(reader.GetString());

    public override void Write(Utf8JsonWriter writer, ConcurrencyToken value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
