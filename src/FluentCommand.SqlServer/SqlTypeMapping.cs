namespace FluentCommand;

/// <summary>
/// Sql Server type mapping
/// </summary>
public static class SqlTypeMapping
{
    private static readonly Dictionary<Type, string> _nativeType = new Dictionary<Type, string>
    {
        {typeof(bool), "bit"},
        {typeof(byte), "tinyint"},
        {typeof(short), "smallint"},
        {typeof(int), "int"},
        {typeof(long), "bigint"},
        {typeof(float), "real"},
        {typeof(double), "float"},
        {typeof(decimal), "decimal"},
        {typeof(byte[]), "varbinary(MAX)"},
        {typeof(string), "nvarchar(MAX)"},
        {typeof(TimeSpan), "time"},
        {typeof(DateTime), "datetime2"},
        {typeof(DateTimeOffset), "datetimeoffset"},
        {typeof(Guid), "uniqueidentifier"},
        #if NET6_0_OR_GREATER
        {typeof(DateOnly), "date"},
        {typeof(TimeOnly), "time"},
        #endif
    };

    /// <summary>
    /// Converts the .NET Type into sql server native type
    /// </summary>
    /// <typeparam name="T">The type to convert</typeparam>
    /// <returns>The SQL Server native type</returns>
    public static string NativeType<T>()
    {
        return NativeType(typeof(T));
    }

    /// <summary>
    /// Converts the .NET Type into sql server native type
    /// </summary>
    /// <param name="type">The type to convert</param>
    /// <returns>The SQL Server native type</returns>
    public static string NativeType(Type type)
    {
        var dataType = Nullable.GetUnderlyingType(type) ?? type;

        _nativeType.TryGetValue(dataType, out var value);

        return value ?? "sql_variant";
    }
}
