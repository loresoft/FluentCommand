using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluentCommand;

/// <summary>
/// Json Converter for <see cref="ConcurrencyToken"/>
/// </summary>
public class ConcurrencyTokenJsonConverter : JsonConverter<ConcurrencyToken>
{
    /// <summary>
    /// Read and convert the JSON to T.
    /// </summary>
    /// <param name="reader">The <see cref="T:System.Text.Json.Utf8JsonReader" /> to read from.</param>
    /// <param name="typeToConvert">The <see cref="T:System.Type" /> being converted.</param>
    /// <param name="options">The <see cref="T:System.Text.Json.JsonSerializerOptions" /> being used.</param>
    /// <returns>
    /// The value that was converted.
    /// </returns>
    /// <remarks>
    /// A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON is invalid.
    /// </remarks>
    public override ConcurrencyToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString());

    /// <summary>
    /// Write the value as JSON.
    /// </summary>
    /// <param name="writer">The <see cref="T:System.Text.Json.Utf8JsonWriter" /> to write to.</param>
    /// <param name="value">The value to convert. Note that the value of <seealso cref="P:System.Text.Json.Serialization.JsonConverter`1.HandleNull" /> determines if the converter handles <see langword="null" /> values.</param>
    /// <param name="options">The <see cref="T:System.Text.Json.JsonSerializerOptions" /> being used.</param>
    /// <remarks>
    /// A converter may throw any Exception, but should throw <cref>JsonException</cref> when the JSON
    /// cannot be created.
    /// </remarks>
    public override void Write(Utf8JsonWriter writer, ConcurrencyToken value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
