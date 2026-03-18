using System.Collections;
using System.Collections.Concurrent;
using System.Data;

using FluentCommand.Extensions;
using FluentCommand.Reflection;

using Microsoft.Data.SqlClient.Server;

namespace FluentCommand;

/// <summary>
/// Adapts an <see cref="IEnumerable{T}"/> to an <see cref="IEnumerable{SqlDataRecord}"/> for use
/// as a SQL Server table-valued parameter. Reuses a single <see cref="SqlDataRecord"/> per row
/// for minimal allocation. Caches <see cref="SqlMetaData"/> per type for efficiency.
/// </summary>
/// <typeparam name="T">The type of items being adapted.</typeparam>
public class SqlDataRecordAdapter<T> : IEnumerable<SqlDataRecord> where T : class
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly ConcurrentDictionary<Type, (SqlMetaData[] MetaData, IMemberAccessor[] Columns)> _metaDataCache = new();

    private readonly IEnumerable<T> _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlDataRecordAdapter{T}"/> class.
    /// </summary>
    /// <param name="source">The source collection to adapt.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
    public SqlDataRecordAdapter(IEnumerable<T> source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <inheritdoc/>
    public IEnumerator<SqlDataRecord> GetEnumerator()
    {
        var (metaData, columns) = GetCachedMetaData();
        var record = new SqlDataRecord(metaData);

        foreach (var item in _source)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                var value = columns[i].GetValue(item);
                if (value is null)
                    record.SetDBNull(i);
                else
                    record.SetValue(i, value);
            }

            yield return record;
        }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static (SqlMetaData[] MetaData, IMemberAccessor[] Columns) GetCachedMetaData()
    {
        return _metaDataCache.GetOrAdd(typeof(T), _ => BuildMetaData());
    }

    private static (SqlMetaData[] MetaData, IMemberAccessor[] Columns) BuildMetaData()
    {
        var typeAccessor = TypeAccessor.GetAccessor<T>();
        var properties = typeAccessor.GetProperties().ToList();

        var metaData = new SqlMetaData[properties.Count];
        var columns = new IMemberAccessor[properties.Count];

        for (int i = 0; i < properties.Count; i++)
        {
            var property = properties[i];
            columns[i] = property;

            var underlyingType = property.MemberType.GetUnderlyingType();
            var sqlDbType = SqlTypeMapping.DbType(underlyingType);

            metaData[i] = sqlDbType switch
            {
                SqlDbType.NVarChar => new SqlMetaData(property.Column, sqlDbType, -1),
                SqlDbType.VarBinary => new SqlMetaData(property.Column, sqlDbType, -1),
                SqlDbType.Decimal => new SqlMetaData(property.Column, sqlDbType, 18, 6),
                _ => new SqlMetaData(property.Column, sqlDbType)
            };
        }

        return (metaData, columns);
    }
}
