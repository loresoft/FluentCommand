using System.Data;

namespace FluentCommand;

/// <summary>
/// Provides mapping between .NET types and their corresponding SQL Server native types.
/// </summary>
public static class SqlTypeMapping
{
    private static readonly Dictionary<Type, string> _nativeType = new()
    {
        {typeof(bool), "bit"},
        {typeof(sbyte), "smallint"},
        {typeof(byte), "tinyint"},
        {typeof(short), "smallint"},
        {typeof(ushort), "int"},
        {typeof(int), "int"},
        {typeof(uint), "bigint"},
        {typeof(long), "bigint"},
        {typeof(ulong), "decimal(20,0)"},
        {typeof(float), "real"},
        {typeof(double), "float"},
        {typeof(decimal), "decimal"},
        {typeof(byte[]), "varbinary(MAX)"},
        {typeof(string), "nvarchar(MAX)"},
        {typeof(char), "nchar(1)"},
        {typeof(TimeSpan), "time"},
        {typeof(DateTime), "datetime2"},
        {typeof(DateTimeOffset), "datetimeoffset"},
        {typeof(Guid), "uniqueidentifier"},
        #if NET6_0_OR_GREATER
        {typeof(DateOnly), "date"},
        {typeof(TimeOnly), "time"},
        #endif
    };

    private static readonly Dictionary<Type, SqlDbType> _dbType = new()
    {
        {typeof(bool), SqlDbType.Bit},
        {typeof(sbyte), SqlDbType.SmallInt},
        {typeof(byte), SqlDbType.TinyInt},
        {typeof(short), SqlDbType.SmallInt},
        {typeof(ushort), SqlDbType.Int},
        {typeof(int), SqlDbType.Int},
        {typeof(uint), SqlDbType.BigInt},
        {typeof(long), SqlDbType.BigInt},
        {typeof(ulong), SqlDbType.Decimal},
        {typeof(float), SqlDbType.Real},
        {typeof(double), SqlDbType.Float},
        {typeof(decimal), SqlDbType.Decimal},
        {typeof(byte[]), SqlDbType.VarBinary},
        {typeof(string), SqlDbType.NVarChar},
        {typeof(char), SqlDbType.NChar},
        {typeof(TimeSpan), SqlDbType.Time},
        {typeof(DateTime), SqlDbType.DateTime2},
        {typeof(DateTimeOffset), SqlDbType.DateTimeOffset},
        {typeof(Guid), SqlDbType.UniqueIdentifier},
        #if NET6_0_OR_GREATER
        {typeof(DateOnly), SqlDbType.Date},
        {typeof(TimeOnly), SqlDbType.Time},
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
        dataType = dataType.IsEnum ? Enum.GetUnderlyingType(dataType) : dataType;

        _nativeType.TryGetValue(dataType, out var value);

        return value ?? "sql_variant";
    }

    /// <summary>
    /// Gets the SQL Server <see cref="SqlDbType"/> for the specified generic .NET type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The .NET type to map to a SQL Server <see cref="SqlDbType"/>.</typeparam>
    /// <returns>
    /// The SQL Server <see cref="SqlDbType"/> value.
    /// Returns <see cref="SqlDbType.NVarChar"/> if the type is not explicitly mapped.
    /// </returns>
    public static SqlDbType DbType<T>()
    {
        return DbType(typeof(T));
    }

    /// <summary>
    /// Gets the SQL Server <see cref="SqlDbType"/> for the specified <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The .NET <see cref="Type"/> to map to a SQL Server <see cref="SqlDbType"/>.</param>
    /// <returns>
    /// The SQL Server <see cref="SqlDbType"/> value.
    /// Returns <see cref="SqlDbType.NVarChar"/> if the type is not explicitly mapped.
    /// </returns>
    public static SqlDbType DbType(Type type)
    {
        var dataType = Nullable.GetUnderlyingType(type) ?? type;
        dataType = dataType.IsEnum ? Enum.GetUnderlyingType(dataType) : dataType;

        return _dbType.TryGetValue(dataType, out var value) ? value : SqlDbType.NVarChar;
    }
}
