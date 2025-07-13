using System.Text.Json.Serialization;

namespace FluentCommand.Import;

[JsonSerializable(typeof(ImportDefinition))]
[JsonSerializable(typeof(ImportData))]
[JsonSerializable(typeof(ImportFieldMapping))]
[JsonSerializable(typeof(FieldDefinition))]
[JsonSerializable(typeof(FieldMap))]
[JsonSerializable(typeof(ImportResult))]
public partial class ImportJsonContext : JsonSerializerContext;
