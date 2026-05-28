namespace FluentCommand.Generators.Models;

public record EntityProperty
{
    public string PropertyName { get; init; } = null!;
    public string ColumnName { get; init; } = null!;
    public string PropertyType { get; init; } = null!;
    public string MemberTypeName { get; init; } = null!;
    public string? ParameterName { get; init; }
    public string? ConverterName { get; init; }
    public bool IsKey { get; init; }
    public bool IsNotMapped { get; init; }
    public bool IsDatabaseGenerated { get; init; }
    public bool IsConcurrencyCheck { get; init; }
    public string? ForeignKey { get; init; }
    public bool IsRequired { get; init; }
    public bool HasGetter { get; init; } = true;
    public bool HasSetter { get; init; } = true;
    public string? DisplayName { get; init; }
    public string? DataFormatString { get; init; }
    public string? ColumnType { get; init; }
    public int? ColumnOrder { get; init; }
    public bool IsJsonColumn { get; init; }
    public string? JsonOptionsProviderName { get; init; }
    public string? JsonContextName { get; init; }
    public string? JsonTypeInfoPropertyName { get; init; }
}
