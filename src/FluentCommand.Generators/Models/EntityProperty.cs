namespace FluentCommand.Generators.Models;

public record EntityProperty(
    string PropertyName,
    string ColumnName,
    string PropertyType,
    string? ParameterName = null,
    string? ConverterName = null,
    bool IsKey = false,
    bool IsNotMapped = false,
    bool IsDatabaseGenerated = false,
    bool IsConcurrencyCheck = false,
    string? ForeignKey = null,
    bool IsRequired = false,
    bool HasGetter = true,
    bool HasSetter = true,
    string? DisplayName = null,
    string? DataFormatString = null,
    string? ColumnType = null,
    int? ColumnOrder = null
);
