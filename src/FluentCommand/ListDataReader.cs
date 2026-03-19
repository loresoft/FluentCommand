using System.Collections;
using System.Data;
using System.Data.Common;

using FluentCommand.Extensions;
using FluentCommand.Reflection;

namespace FluentCommand;

/// <summary>
/// Read a list of items using a <see cref="DbDataReader"/>
/// </summary>
/// <typeparam name="T">The type of items being read</typeparam>
public class ListDataReader<T> : DbDataReader where T : class
{
    // ReSharper disable once StaticMemberInGenericType
    private static readonly TypeAccessor _typeAccessor;

    private readonly IEnumerator<T> _iterator;
    private readonly List<IMemberAccessor> _activeColumns;
    private readonly Dictionary<string, int> _columnOrdinals;
    private bool _closed;
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
    public ListDataReader(IEnumerable<T> list, IEnumerable<string>? ignoreNames = null)
    {
        if (list is null)
            throw new ArgumentNullException(nameof(list));

        _iterator = list.GetEnumerator();

        var ignored = new HashSet<string>(ignoreNames ?? []);

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
    public override int Depth => 0;

    /// <summary>
    /// Gets a value indicating whether the data reader is closed.
    /// </summary>
    public override bool IsClosed => _closed;

    /// <summary>
    /// Gets the number of rows affected. Always returns 0.
    /// </summary>
    public override int RecordsAffected => 0;

    /// <summary>
    /// Gets a value indicating whether the reader has rows. Always returns false.
    /// </summary>
    public override bool HasRows => false;

    /// <summary>
    /// Returns a <see cref="DataTable"/> that describes the column metadata of the data reader.
    /// </summary>
    /// <returns>A <see cref="DataTable"/> describing the column metadata.</returns>
    public override DataTable GetSchemaTable()
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
    public override bool NextResult() => false;

    /// <summary>
    /// Advances the data reader to the next record.
    /// </summary>
    /// <returns>true if there are more rows; otherwise, false.</returns>
    public override bool Read() => _iterator.MoveNext();

    /// <summary>
    /// Returns an enumerator that iterates through the rows of the data reader.
    /// </summary>
    /// <returns>An <see cref="IEnumerator"/> for the data reader.</returns>
    public override IEnumerator GetEnumerator() => new DbEnumerator(this);

    /// <summary>
    /// Closes the data reader and releases resources.
    /// </summary>
    public override void Close()
    {
        Dispose(true);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the ListDataReader and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _iterator.Dispose();
            _closed = true;
        }

        _disposed = true;
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public override string GetName(int i) => _activeColumns[i].Column;

    /// <inheritdoc/>
    public override string GetDataTypeName(int i) => _activeColumns[i].MemberType.Name;

    /// <inheritdoc/>
    public override Type GetFieldType(int i) => _activeColumns[i].MemberType;

    /// <inheritdoc/>
    public override object GetValue(int i) => _activeColumns[i].GetValue(_iterator.Current)!;

    /// <inheritdoc/>
    public override int GetValues(object[] values)
    {
        int count = Math.Min(_activeColumns.Count, values.Length);
        for (int i = 0; i < count; i++)
            values[i] = GetValue(i);
        return count;
    }

    /// <inheritdoc/>
    public override int GetOrdinal(string name)
    {
        if (_columnOrdinals.TryGetValue(name, out int ordinal))
            return ordinal;

        return -1;
    }

    /// <inheritdoc/>
    public override bool GetBoolean(int i) => (bool)GetValue(i);

    /// <inheritdoc/>
    public override byte GetByte(int i) => (byte)GetValue(i);

    /// <inheritdoc/>
    public override long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferOffset, int length)
    {
        byte[] value = (byte[])GetValue(i);

        int available = value.Length - (int)fieldOffset;
        if (available <= 0)
            return 0;

        int count = Math.Min(length, available);

        if (buffer != null)
            Buffer.BlockCopy(value, (int)fieldOffset, buffer, bufferOffset, count);

        return count;
    }

    /// <inheritdoc/>
    public override char GetChar(int i) => (char)GetValue(i);

    /// <inheritdoc/>
    public override long GetChars(int i, long fieldOffset, char[]? buffer, int bufferOffset, int length)
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
    public override Guid GetGuid(int i) => (Guid)GetValue(i);

    /// <inheritdoc/>
    public override short GetInt16(int i) => (short)GetValue(i);

    /// <inheritdoc/>
    public override int GetInt32(int i) => (int)GetValue(i);

    /// <inheritdoc/>
    public override long GetInt64(int i) => (long)GetValue(i);

    /// <inheritdoc/>
    public override float GetFloat(int i) => (float)GetValue(i);

    /// <inheritdoc/>
    public override double GetDouble(int i) => (double)GetValue(i);

    /// <inheritdoc/>
    public override string GetString(int i) => (string)GetValue(i);

    /// <inheritdoc/>
    public override decimal GetDecimal(int i) => (decimal)GetValue(i);

    /// <inheritdoc/>
    public override DateTime GetDateTime(int i) => (DateTime)GetValue(i);

    /// <inheritdoc/>
    public override bool IsDBNull(int i)
    {
        var value = GetValue(i);
        return value == null || value == DBNull.Value;
    }

    /// <inheritdoc/>
    public override int FieldCount => _activeColumns.Count;

    /// <inheritdoc/>
    public override object this[int i] => GetValue(i);

    /// <inheritdoc/>
    public override object this[string name] => GetValue(GetOrdinal(name));
}
