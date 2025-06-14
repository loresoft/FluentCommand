using System.Data;

namespace FluentCommand.Merge;

/// <summary>
/// Wraps an <see cref="IDataReader"/> and optionally applies a prefix to field names when accessing data.
/// </summary>
public class DataReaderWrapper : IDataReader
{
    private readonly IDataReader _dataReader;
    private readonly string _fieldPrefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataReaderWrapper"/> class with the specified data reader.
    /// </summary>
    /// <param name="dataReader">The underlying <see cref="IDataReader"/> to wrap.</param>
    public DataReaderWrapper(IDataReader dataReader)
        : this(dataReader, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataReaderWrapper"/> class with the specified data reader and field prefix.
    /// </summary>
    /// <param name="dataReader">The underlying <see cref="IDataReader"/> to wrap.</param>
    /// <param name="fieldPrefix">The prefix to apply to field names when accessing data, or <c>null</c> for no prefix.</param>
    public DataReaderWrapper(IDataReader dataReader, string fieldPrefix)
    {
        _dataReader = dataReader;
        _fieldPrefix = fieldPrefix;
    }

    /// <summary>
    /// Gets the name of the field at the specified index.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The name of the field, or an empty string if there is no value to return.</returns>
    public string GetName(int i) => _dataReader.GetName(i);

    /// <summary>
    /// Gets the data type information for the specified field.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The data type information for the specified field.</returns>
    public string GetDataTypeName(int i) => _dataReader.GetDataTypeName(i);

    /// <summary>
    /// Gets the <see cref="Type"/> information corresponding to the type of <see cref="object"/> that would be returned from <see cref="GetValue(int)"/>.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The <see cref="Type"/> of the field value.</returns>
    public Type GetFieldType(int i) => _dataReader.GetFieldType(i);

    /// <summary>
    /// Gets the value of the specified field.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The value of the field as an <see cref="object"/>.</returns>
    public object GetValue(int i) => _dataReader.GetValue(i);

    /// <summary>
    /// Populates an array of objects with the column values of the current record.
    /// </summary>
    /// <param name="values">An array of <see cref="object"/> to copy the attribute fields into.</param>
    /// <returns>The number of objects in the array.</returns>
    public int GetValues(object[] values) => _dataReader.GetValues(values);

    /// <summary>
    /// Gets the index of the field with the specified name, applying the field prefix if set.
    /// </summary>
    /// <param name="name">The name of the field to find.</param>
    /// <returns>The zero-based index of the named field.</returns>
    public int GetOrdinal(string name)
    {
        string prefixName = _fieldPrefix != null
            ? _fieldPrefix + name
            : name;

        return _dataReader.GetOrdinal(prefixName);
    }

    /// <summary>
    /// Gets the value of the specified column as a <see cref="bool"/>.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <returns>The value of the column as a <see cref="bool"/>.</returns>
    public bool GetBoolean(int i) => _dataReader.GetBoolean(i);

    /// <summary>
    /// Gets the 8-bit unsigned integer value of the specified column.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <returns>The 8-bit unsigned integer value of the specified column.</returns>
    public byte GetByte(int i) => _dataReader.GetByte(i);

    /// <summary>
    /// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
    /// <param name="bufferOffset">The index for buffer to start the read operation.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>The actual number of bytes read.</returns>
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length) => _dataReader.GetBytes(i, fieldOffset, buffer, bufferOffset, length);

    /// <summary>
    /// Gets the character value of the specified column.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <returns>The character value of the specified column.</returns>
    public char GetChar(int i) => _dataReader.GetChar(i);

    /// <summary>
    /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <param name="fieldOffset">The index within the row from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of characters.</param>
    /// <param name="bufferOffset">The index for buffer to start the read operation.</param>
    /// <param name="length">The number of characters to read.</param>
    /// <returns>The actual number of characters read.</returns>
    public long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length) => _dataReader.GetChars(i, fieldOffset, buffer, bufferOffset, length);

    /// <summary>
    /// Gets the <see cref="Guid"/> value of the specified field.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The <see cref="Guid"/> value of the specified field.</returns>
    public Guid GetGuid(int i) => _dataReader.GetGuid(i);

    /// <summary>
    /// Gets the 16-bit signed integer value of the specified field.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The 16-bit signed integer value of the specified field.</returns>
    public short GetInt16(int i) => _dataReader.GetInt16(i);

    /// <summary>
    /// Gets the 32-bit signed integer value of the specified field.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The 32-bit signed integer value of the specified field.</returns>
    public int GetInt32(int i) => _dataReader.GetInt32(i);

    /// <summary>
    /// Gets the 64-bit signed integer value of the specified field.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The 64-bit signed integer value of the specified field.</returns>
    public long GetInt64(int i) => _dataReader.GetInt64(i);

    /// <summary>
    /// Gets the single-precision floating point number of the specified field.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The single-precision floating point number of the specified field.</returns>
    public float GetFloat(int i) => _dataReader.GetFloat(i);

    /// <summary>
    /// Gets the double-precision floating point number of the specified field.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The double-precision floating point number of the specified field.</returns>
    public double GetDouble(int i) => _dataReader.GetDouble(i);

    /// <summary>
    /// Gets the string value of the specified field.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The string value of the specified field.</returns>
    public string GetString(int i) => _dataReader.GetString(i);

    /// <summary>
    /// Gets the fixed-position numeric value of the specified field.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The fixed-position numeric value of the specified field.</returns>
    public decimal GetDecimal(int i) => _dataReader.GetDecimal(i);

    /// <summary>
    /// Gets the date and time data value of the specified field.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The date and time data value of the specified field.</returns>
    public DateTime GetDateTime(int i) => _dataReader.GetDateTime(i);

    /// <summary>
    /// Returns an <see cref="IDataReader"/> for the specified column ordinal.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>An <see cref="IDataReader"/> for the specified column ordinal.</returns>
    public IDataReader GetData(int i) => _dataReader.GetData(i);

    /// <summary>
    /// Determines whether the specified field is set to null.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns><c>true</c> if the specified field is set to null; otherwise, <c>false</c>.</returns>
    public bool IsDBNull(int i) => _dataReader.IsDBNull(i);

    /// <summary>
    /// Gets the number of columns in the current row.
    /// </summary>
    public int FieldCount => _dataReader.FieldCount;

    /// <summary>
    /// Gets the value of the field at the specified index.
    /// </summary>
    /// <param name="i">The zero-based index of the field.</param>
    /// <returns>The value of the field as an <see cref="object"/>.</returns>
    object IDataRecord.this[int i] => _dataReader[i];

    /// <summary>
    /// Gets the value of the field with the specified name, applying the field prefix if set.
    /// </summary>
    /// <param name="name">The name of the field.</param>
    /// <returns>The value of the field as an <see cref="object"/>.</returns>
    object IDataRecord.this[string name]
    {
        get
        {
            string prefixName = _fieldPrefix != null
                ? _fieldPrefix + name
                : name;

            return _dataReader[prefixName];
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="DataReaderWrapper"/> and the underlying <see cref="IDataReader"/>.
    /// </summary>
    public void Dispose() => _dataReader.Dispose();

    /// <summary>
    /// Closes the underlying <see cref="IDataReader"/>.
    /// </summary>
    public void Close() => _dataReader.Dispose();

    /// <summary>
    /// Returns a <see cref="DataTable"/> that describes the column metadata of the <see cref="IDataReader"/>.
    /// </summary>
    /// <returns>A <see cref="DataTable"/> that describes the column metadata.</returns>
    public DataTable GetSchemaTable() => _dataReader.GetSchemaTable();

    /// <summary>
    /// Advances the data reader to the next result, when reading the results of batch SQL statements.
    /// </summary>
    /// <returns><c>true</c> if there are more result sets; otherwise, <c>false</c>.</returns>
    public bool NextResult() => _dataReader.NextResult();

    /// <summary>
    /// Advances the <see cref="IDataReader"/> to the next record.
    /// </summary>
    /// <returns><c>true</c> if there are more rows; otherwise, <c>false</c>.</returns>
    public bool Read() => _dataReader.Read();

    /// <summary>
    /// Gets a value indicating the depth of nesting for the current row.
    /// </summary>
    public int Depth => _dataReader.Depth;

    /// <summary>
    /// Gets a value indicating whether the data reader is closed.
    /// </summary>
    public bool IsClosed => _dataReader.IsClosed;

    /// <summary>
    /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
    /// </summary>
    public int RecordsAffected => _dataReader.RecordsAffected;
}
