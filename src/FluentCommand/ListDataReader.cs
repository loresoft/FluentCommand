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
    }

    /// <inheritdoc/>
    public int Depth => 0;

    /// <inheritdoc/>
    public bool IsClosed { get; private set; }

    /// <inheritdoc/>
    public int RecordsAffected => 0;

    /// <inheritdoc/>
    public void Close()
    {
        _iterator.Dispose();
        IsClosed = true;
    }

    /// <inheritdoc/>
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

        var rowData = new object[5];
        for (int i = 0; i < _activeColumns.Count; i++)
        {
            rowData[0] = i;
            rowData[1] = _activeColumns[i].Column;
            rowData[2] = _activeColumns[i].MemberType.GetUnderlyingType();
            rowData[3] = -1;
            rowData[4] = _activeColumns[i].MemberType.IsNullable();

            table.Rows.Add(rowData);
        }

        return table;
    }

    /// <inheritdoc/>
    public bool NextResult() => false;

    /// <inheritdoc/>
    public bool Read() => _iterator.MoveNext();

    /// <inheritdoc/>
    public void Dispose() => Close();

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
        int count = Math.Max(_activeColumns.Count, values.Length);

        for (int i = 0; i < count; i++)
            values[i] = GetValue(i);

        return count;
    }

    /// <inheritdoc/>
    public int GetOrdinal(string name) => _activeColumns.FindIndex(p => p.Column == name);

    /// <inheritdoc/>
    public bool GetBoolean(int i) => (bool)GetValue(i);

    /// <inheritdoc/>
    public byte GetByte(int i) => (byte)GetValue(i);

    /// <inheritdoc/>
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
    {
        byte[] value = (byte[])GetValue(i);

        int available = value.Length - (int)fieldOffset;
        if (available <= 0)
            return 0;

        int count = Math.Min(length, available);

        Buffer.BlockCopy(value, (int)fieldOffset, buffer, bufferoffset, count);

        return count;
    }

    /// <inheritdoc/>
    public char GetChar(int i) => (char)GetValue(i);

    /// <inheritdoc/>
    public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        string value = (string)GetValue(i);

        int available = value.Length - (int)fieldoffset;
        if (available <= 0)
            return 0;

        int count = Math.Min(length, available);

        value.CopyTo((int)fieldoffset, buffer, bufferoffset, count);

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
    public bool IsDBNull(int i) => GetValue(i) == DBNull.Value;

    /// <inheritdoc/>
    public int FieldCount => _activeColumns.Count;

    /// <inheritdoc/>
    public object this[int i] => GetValue(i);

    /// <inheritdoc/>
    public object this[string name] => GetValue(GetOrdinal(name));
}
