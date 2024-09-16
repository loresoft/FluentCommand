namespace FluentCommand.Generators.Models;

public record EntityProperty(
    string PropertyName,
    string ColumnName,
    string PropertyType,
    string ParameterName = null,
    string ConverterName = null
);
