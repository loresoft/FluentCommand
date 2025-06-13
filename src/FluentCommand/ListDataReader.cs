using System.Data;

using FluentCommand.Extensions;
using FluentCommand.Reflection;

namespace FluentCommand;

/// <summary>
/// Read a list of items using an <see cref="IDataReader"/>
/// </summary>
/// <typeparam name="T">The type of items being read</typeparam>
public class ListDataReader<T> : IDataReader where T : class
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly TypeAccessor _typeAccessor;

    private readonly IEnumerator<T> _iterator;
    private readonly List<IMemberAccessor> _activeColumns;
    private readonly Dictionary<string, int> _columnOrdinals;
    private bool _disposed;

    static ListDataReader()
    {
        _typeAccessor = TypeAccessor.GetAccessor<T>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ListDataReader{T}"/> class.
    /// </summary>
    /// <param name="list">The list of items to read</param>
    /// <param name="ignoreNames">A list of property names to ignore in the reader</param>
    public ListDataReader(IEnumerable<T> list, IEnumerable<string> ignoreNames = null)
    {
        if (list is null)
            throw new ArgumentNullException(nameof(list));

        _iterator = list.GetEnumerator();

        var ignored = new HashSet<string>(ignoreNames ?? Enumerable.Empty<string>());

        _activeColumns = _typeAccessor
            .GetProperties()
            .Where(c => !ignored.Contains(c.Name))
            .ToList();

        _columnOrdinals = _activeColumns
            .Select((col, idx) => new { col.Column, idx })
            .ToDictionary(x => x.Column, x => x.idx, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a value indicating the depth of nesting for the current row. Always returns 0.
    /// </summary>
    public int Depth => 0;

    /// <summary>
    /// Gets a value indicating whether the data reader is closed.
    /// </summary>
    public bool IsClosed { get; private set; }

    /// <summary>
    /// Gets the number of rows affected. Always returns 0.
    /// </summary>
    public int RecordsAffected => 0;

    /// <summary>
    /// Closes the data reader and releases resources.
    /// </summary>
    public void Close()
    {
        Dispose(true);
    }

    /// <summary>
    /// Returns a <see cref="DataTable"/> that describes the column metadata of the data reader.
    /// </summary>
    /// <returns>A <see cref="DataTable"/> describing the column metadata.</returns>
    public DataTable GetSchemaTable()
    {
        // these are the columns used by DataTable load
        var table = new DataTable
        {
            Columns =
            {
                {"ColumnOrdinal", typeof(int)},
                {"ColumnName", typeof(string)},
                {"DataType", typeof(Type)},
                {"ColumnSize", typeof(int)},
                {"AllowDBNull", typeof(bool)}
            }
        };

        for (int i = 0; i < _activeColumns.Count; i++)
        {
            var rowData = new object[5];
            rowData[0] = i;
            rowData[1] = _activeColumns[i].Column;
            rowData[2] = _activeColumns[i].MemberType.GetUnderlyingType();
            rowData[3] = -1;
            rowData[4] = _activeColumns[i].MemberType.IsNullable();

            table.Rows.Add(rowData);
        }

        return table;
    }

    /// <summary>
    /// Advances the data reader to the next result. Always returns false.
    /// </summary>
    /// <returns>false, as multiple result sets are not supported.</returns>
    public bool NextResult() => false;

    /// <summary>
    /// Advances the data reader to the next record.
    /// </summary>
    /// <returns>true if there are more rows; otherwise, false.</returns>
    public bool Read() => _iterator.MoveNext();

    /// <summary>
    /// Releases all resources used by the <see cref="ListDataReader{T}"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the ListDataReader and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            if (!IsClosed)
            {
                _iterator.Dispose();
                IsClosed = true;
            }
        }

        _disposed = true;
    }

    /// <inheritdoc/>
    public string GetName(int i) => _activeColumns[i].Column;

    /// <inheritdoc/>
    public string GetDataTypeName(int i) => _activeColumns[i].MemberType.Name;

    /// <inheritdoc/>
    public Type GetFieldType(int i) => _activeColumns[i].MemberType;

    /// <inheritdoc/>
    public object GetValue(int i) => _activeColumns[i].GetValue(_iterator.Current);

    /// <inheritdoc/>
    public int GetValues(object[] values)
    {
        int count = Math.Min(_activeColumns.Count, values.Length);
        for (int i = 0; i < count; i++)
            values[i] = GetValue(i);
        return count;
    }

    /// <inheritdoc/>
    public int GetOrdinal(string name)
    {
        if (_columnOrdinals.TryGetValue(name, out int ordinal))
            return ordinal;

        return -1;
    }

    /// <inheritdoc/>
    public bool GetBoolean(int i) => (bool)GetValue(i);

    /// <inheritdoc/>
    public byte GetByte(int i) => (byte)GetValue(i);

    /// <inheritdoc/>
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
    {
        byte[] value = (byte[])GetValue(i);

        int available = value.Length - (int)fieldOffset;
        if (available <= 0)
            return 0;

        int count = Math.Min(length, available);

        Buffer.BlockCopy(value, (int)fieldOffset, buffer, bufferOffset, count);

        return count;
    }

    /// <inheritdoc/>
    public char GetChar(int i) => (char)GetValue(i);

    /// <inheritdoc/>
    public long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        string value = (string)GetValue(i);

        int available = value.Length - (int)fieldOffset;
        if (available <= 0)
            return 0;

        int count = Math.Min(length, available);

        value.CopyTo((int)fieldOffset, buffer, bufferOffset, count);

        return count;
    }

    /// <inheritdoc/>
    public Guid GetGuid(int i) => (Guid)GetValue(i);

    /// <inheritdoc/>
    public short GetInt16(int i) => (short)GetValue(i);

    /// <inheritdoc/>
    public int GetInt32(int i) => (int)GetValue(i);

    /// <inheritdoc/>
    public long GetInt64(int i) => (long)GetValue(i);

    /// <inheritdoc/>
    public float GetFloat(int i) => (float)GetValue(i);

    /// <inheritdoc/>
    public double GetDouble(int i) => (double)GetValue(i);

    /// <inheritdoc/>
    public string GetString(int i) => (string)GetValue(i);

    /// <inheritdoc/>
    public decimal GetDecimal(int i) => (decimal)GetValue(i);

    /// <inheritdoc/>
    public DateTime GetDateTime(int i) => (DateTime)GetValue(i);

    /// <inheritdoc/>
    public IDataReader GetData(int i) => throw new NotImplementedException();

    /// <inheritdoc/>
    public bool IsDBNull(int i)
    {
        var value = GetValue(i);
        return value == null || value == DBNull.Value;
    }

    /// <inheritdoc/>
    public int FieldCount => _activeColumns.Count;

    /// <inheritdoc/>
    public object this[int i] => GetValue(i);

    /// <inheritdoc/>
    public object this[string name] => GetValue(GetOrdinal(name));
}
