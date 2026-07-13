using System.Text.Json;
using System.Data;
using System.Diagnostics.CodeAnalysis;

using FluentCommand.Handlers;

namespace FluentCommand.Tests.Handlers;

public class JsonElementHandlerTests
{
    [Fact]
    public void ReadValue_WithJsonObject_ReturnsJsonElement()
    {
        var handler = new JsonElementHandler();
        using var reader = new ListDataReader<JsonPayloadRow>([
            new() { Payload = """{"name":"First","active":true}""" }
        ]);

        reader.Read().Should().BeTrue();

        var result = handler.ReadValue(reader, reader.GetOrdinal(nameof(JsonPayloadRow.Payload)));

        result.ValueKind.Should().Be(JsonValueKind.Object);
        result.GetProperty("name").GetString().Should().Be("First");
        result.GetProperty("active").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void ReadValue_WithNull_ReturnsUndefinedJsonElement()
    {
        var handler = new JsonElementHandler();
        using var reader = new ListDataReader<JsonPayloadRow>([
            new() { Payload = null }
        ]);

        reader.Read().Should().BeTrue();

        var result = handler.ReadValue(reader, reader.GetOrdinal(nameof(JsonPayloadRow.Payload)));

        result.ValueKind.Should().Be(JsonValueKind.Undefined);
    }

    [Fact]
    public void ReadValue_WithDBNull_ReturnsUndefinedJsonElement()
    {
        var handler = new JsonElementHandler();
        using var reader = new ListDataReader<JsonPayloadRow>([
            new() { Payload = DBNull.Value }
        ]);

        reader.Read().Should().BeTrue();

        var result = handler.ReadValue(reader, reader.GetOrdinal(nameof(JsonPayloadRow.Payload)));

        result.ValueKind.Should().Be(JsonValueKind.Undefined);
    }

    [Fact]
    public void SetValue_WithJsonElement_SetsRawJsonString()
    {
        var handler = new JsonElementHandler();
        using var document = JsonDocument.Parse("""{"name":"First"}""");
        var parameter = new TestDataParameter();

        handler.SetValue(parameter, document.RootElement);

        parameter.DbType.Should().Be(System.Data.DbType.String);
        parameter.Value.Should().Be("""{"name":"First"}""");
    }

    private sealed class JsonPayloadRow
    {
        public object? Payload { get; set; }
    }

    private sealed class TestDataParameter : IDbDataParameter
    {
        public byte Precision { get; set; }

        public byte Scale { get; set; }

        public int Size { get; set; }

        public DbType DbType { get; set; }

        public ParameterDirection Direction { get; set; }

        public bool IsNullable => true;

        [AllowNull]
        public string ParameterName { get; set; } = string.Empty;

        [AllowNull]
        public string SourceColumn { get; set; } = string.Empty;

        public DataRowVersion SourceVersion { get; set; }

        public object? Value { get; set; }
    }
}
