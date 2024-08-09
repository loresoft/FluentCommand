using System.Data;

namespace FluentCommand;

/// <summary>
/// A class for mapping data types
/// </summary>
public static class DataMapping
{
    private static readonly Dictionary<Type, DbType> _typeToDbType;
    private static readonly Dictionary<DbType, Type> _dbTypeToType;

    /// <summary>
    /// Initializes the <see cref="DataMapping"/> class.
    /// </summary>
    static DataMapping()
    {
        _typeToDbType = new Dictionary<Type, DbType>
        {
            {typeof(byte[]), DbType.Binary},
            {typeof(bool), DbType.Boolean},
            {typeof(byte), DbType.Byte},
            {typeof(DateTime), DbType.DateTime},
            {typeof(DateTimeOffset), DbType.DateTimeOffset},
            {typeof(decimal), DbType.Decimal},
            {typeof(double), DbType.Double},
            {typeof(Guid), DbType.Guid},
            {typeof(short), DbType.Int16},
            {typeof(int), DbType.Int32},
            {typeof(long), DbType.Int64},
            {typeof(sbyte), DbType.SByte},
            {typeof(float), DbType.Single},
            {typeof(string), DbType.String},
            {typeof(TimeSpan), DbType.Time},
            {typeof(ushort), DbType.UInt16},
            {typeof(uint), DbType.UInt32},
            {typeof(ulong), DbType.UInt64},
            #if NET6_0_OR_GREATER
            {typeof(DateOnly), DbType.Date},
            {typeof(TimeOnly), DbType.Time},
            #endif
        };

        _dbTypeToType = new Dictionary<DbType, Type>
        {
            {DbType.AnsiString, typeof(string)},
            {DbType.AnsiStringFixedLength, typeof(string)},
            {DbType.Binary, typeof(byte[])},
            {DbType.Boolean, typeof(bool)},
            {DbType.Byte, typeof(byte)},
            {DbType.Currency, typeof(decimal)},
            {DbType.DateTime, typeof(DateTime)},
            {DbType.Decimal, typeof(decimal)},
            {DbType.Double, typeof(double)},
            {DbType.Guid, typeof(Guid)},
            {DbType.Int16, typeof(short)},
            {DbType.Int32, typeof(int)},
            {DbType.Int64, typeof(long)},
            {DbType.Object, typeof(object)},
            {DbType.SByte, typeof(sbyte)},
            {DbType.Single, typeof(float)},
            {DbType.String, typeof(string)},
            {DbType.StringFixedLength, typeof(string)},
            {DbType.UInt16, typeof(ushort)},
            {DbType.UInt32, typeof(uint)},
            {DbType.UInt64, typeof(ulong)},
            {DbType.VarNumeric, typeof(decimal)},
            {DbType.DateTime2, typeof(DateTime)},
            {DbType.DateTimeOffset, typeof(DateTimeOffset)},
            #if NET6_0_OR_GREATER
            {DbType.Date, typeof(DateOnly)},
            {DbType.Time, typeof(TimeOnly)},
            #else
            {DbType.Date, typeof(DateTime)},
            {DbType.Time, typeof(TimeSpan)},
            #endif

        };
    }

    /// <summary>
    /// Converts system <see cref="Type"/> to a <see cref="DbType"/>.
    /// </summary>
    /// <param name="type">The system type to convert.</param>
    /// <returns>A <see cref="DbType"/> for the system <see cref="Type"/>.</returns>
    public static DbType ToDbType(this Type type)
    {
        return _typeToDbType.TryGetValue(type, out var dbType) ? dbType : DbType.Object;
    }

    /// <summary>
    /// Converts <see cref="DbType"/> to a system <see cref="Type"/>.
    /// </summary>
    /// <param name="dbType">The DbType to convert.</param>
    /// <returns>A system <see cref="Type"/> for the <see cref="DbType"/>.</returns>
    public static Type ToType(this DbType dbType)
    {
        return _dbTypeToType.TryGetValue(dbType, out var type) ? type : typeof(object);
    }
}
