namespace FluentCommand;

/// <summary>
/// Provides mapping between .NET types and their corresponding SQL Server native types.
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
    /// Gets the SQL Server native type name for the specified generic .NET type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The .NET type to map to a SQL Server native type.</typeparam>
    /// <returns>
    /// The SQL Server native type name as a <see cref="string"/>.
    /// Returns <c>sql_variant</c> if the type is not explicitly mapped.
    /// </returns>
    public static string NativeType<T>()
    {
        return NativeType(typeof(T));
    }

    /// <summary>
    /// Gets the SQL Server native type name for the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The .NET <see cref="Type"/> to map to a SQL Server native type.</param>
    /// <returns>
    /// The SQL Server native type name as a <see cref="string"/>.
    /// Returns <c>sql_variant</c> if the type is not explicitly mapped.
    /// </returns>
    public static string NativeType(Type type)
    {
        var dataType = Nullable.GetUnderlyingType(type) ?? type;

        _nativeType.TryGetValue(dataType, out var value);

        return value ?? "sql_variant";
    }
}
