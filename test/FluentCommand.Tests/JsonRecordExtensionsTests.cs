using System.Data;
using System.Text.Json.Serialization;

using FluentCommand.Extensions;

namespace FluentCommand.Tests;

public partial class JsonRecordExtensionsTests
{
    private class JsonItem
    {
        public string? Data { get; set; }
    }

    private record JsonPayload(string Name);

    [JsonSerializable(typeof(JsonPayload))]
    private partial class JsonPayloadContext : JsonSerializerContext;

    [Fact]
    public void GetRequiredFromJson_WhenValueExistsThenReturnsDeserializedValue()
    {
        var item = new JsonItem { Data = "{\"Name\":\"Test\"}" };
        using var reader = new ListDataReader<JsonItem>([item]);

        reader.Read();
        var result = reader.GetRequiredFromJson<JsonPayload>("Data");

        result.Name.Should().Be("Test");
    }

    [Fact]
    public void GetRequiredFromJson_WhenColumnIsNullThenThrowsDataException()
    {
        var item = new JsonItem { Data = null };
        using var reader = new ListDataReader<JsonItem>([item]);

        reader.Read();
        Action act = () => reader.GetRequiredFromJson<JsonPayload>("Data");

        act.Should().Throw<DataException>();
    }

    [Fact]
    public void GetRequiredFromJson_WhenJsonIsNullThenThrowsDataException()
    {
        var item = new JsonItem { Data = "null" };
        using var reader = new ListDataReader<JsonItem>([item]);

        reader.Read();
        Action act = () => reader.GetRequiredFromJson<JsonPayload>("Data");

        act.Should().Throw<DataException>();
    }

    [Fact]
    public void GetRequiredFromJson_WithJsonTypeInfoWhenValueExistsThenReturnsDeserializedValue()
    {
        var item = new JsonItem { Data = "{\"Name\":\"Test\"}" };
        using var reader = new ListDataReader<JsonItem>([item]);

        reader.Read();
        var result = reader.GetRequiredFromJson("Data", JsonPayloadContext.Default.JsonPayload);

        result.Name.Should().Be("Test");
    }
}
