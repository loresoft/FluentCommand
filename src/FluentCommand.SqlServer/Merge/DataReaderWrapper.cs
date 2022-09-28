using System;
using System.Data;

namespace FluentCommand.Merge;

public class DataReaderWrapper : IDataReader
{
    private readonly IDataReader _dataReader;
    private readonly string _fieldPrefix;

    public DataReaderWrapper(IDataReader dataReader)
        : this(dataReader, null)
    {
    }

    public DataReaderWrapper(IDataReader dataReader, string fieldPrefix)
    {
        _dataReader = dataReader;
        _fieldPrefix = fieldPrefix;
    }

    /// <summary>
    /// Gets the name for the field to find.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The name of the field or the empty string (""), if there is no value to return.
    /// </returns>
    public string GetName(int i) => _dataReader.GetName(i);

    /// <summary>
    /// Gets the data type information for the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The data type information for the specified field.
    /// </returns>
    public string GetDataTypeName(int i) => _dataReader.GetDataTypeName(i);

    /// <summary>
    /// Gets the <see cref="T:System.Type"></see> information corresponding to the type of <see cref="T:System.Object"></see> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)"></see>.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The <see cref="T:System.Type"></see> information corresponding to the type of <see cref="T:System.Object"></see> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)"></see>.
    /// </returns>
    public Type GetFieldType(int i) => _dataReader.GetFieldType(i);

    /// <summary>
    /// Return the value of the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The <see cref="T:System.Object"></see> which will contain the field value upon return.
    /// </returns>
    public object GetValue(int i) => _dataReader.GetValue(i);

    /// <summary>
    /// Populates an array of objects with the column values of the current record.
    /// </summary>
    /// <param name="values">An array of <see cref="T:System.Object"></see> to copy the attribute fields into.</param>
    /// <returns>
    /// The number of instances of <see cref="T:System.Object"></see> in the array.
    /// </returns>
    public int GetValues(object[] values) => _dataReader.GetValues(values);

    /// <summary>
    /// Return the index of the named field.
    /// </summary>
    /// <param name="name">The name of the field to find.</param>
    /// <returns>
    /// The index of the named field.
    /// </returns>
    public int GetOrdinal(string name)
    {
        string prefixName = _fieldPrefix != null
            ? _fieldPrefix + name
            : name;

        return _dataReader.GetOrdinal(prefixName);
    }

    /// <summary>
    /// Gets the value of the specified column as a Boolean.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <returns>
    /// The value of the column.
    /// </returns>
    public bool GetBoolean(int i) => _dataReader.GetBoolean(i);

    /// <summary>
    /// Gets the 8-bit unsigned integer value of the specified column.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <returns>
    /// The 8-bit unsigned integer value of the specified column.
    /// </returns>
    public byte GetByte(int i) => _dataReader.GetByte(i);

    /// <summary>
    /// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
    /// <param name="bufferOffset">The index for buffer to start the read operation.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>
    /// The actual number of bytes read.
    /// </returns>
    public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length) => _dataReader.GetBytes(i, fieldOffset, buffer, bufferOffset, length);

    /// <summary>
    /// Gets the character value of the specified column.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <returns>
    /// The character value of the specified column.
    /// </returns>
    public char GetChar(int i) => _dataReader.GetChar(i);

    /// <summary>
    /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
    /// </summary>
    /// <param name="i">The zero-based column ordinal.</param>
    /// <param name="fieldOffset">The index within the row from which to start the read operation.</param>
    /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
    /// <param name="bufferOffset">The index for buffer to start the read operation.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <returns>
    /// The actual number of characters read.
    /// </returns>
    public long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length) => _dataReader.GetChars(i, fieldOffset, buffer, bufferOffset, length);

    /// <summary>
    /// Returns the GUID value of the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The GUID value of the specified field.
    /// </returns>
    public Guid GetGuid(int i) => _dataReader.GetGuid(i);

    /// <summary>
    /// Gets the 16-bit signed integer value of the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The 16-bit signed integer value of the specified field.
    /// </returns>
    public short GetInt16(int i) => _dataReader.GetInt16(i);

    /// <summary>
    /// Gets the 32-bit signed integer value of the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The 32-bit signed integer value of the specified field.
    /// </returns>
    public int GetInt32(int i) => _dataReader.GetInt32(i);

    /// <summary>
    /// Gets the 64-bit signed integer value of the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The 64-bit signed integer value of the specified field.
    /// </returns>
    public long GetInt64(int i) => _dataReader.GetInt64(i);

    /// <summary>
    /// Gets the single-precision floating point number of the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The single-precision floating point number of the specified field.
    /// </returns>
    public float GetFloat(int i) => _dataReader.GetFloat(i);

    /// <summary>
    /// Gets the double-precision floating point number of the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The double-precision floating point number of the specified field.
    /// </returns>
    public double GetDouble(int i) => _dataReader.GetDouble(i);

    /// <summary>
    /// Gets the string value of the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The string value of the specified field.
    /// </returns>
    public string GetString(int i) => _dataReader.GetString(i);

    /// <summary>
    /// Gets the fixed-position numeric value of the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The fixed-position numeric value of the specified field.
    /// </returns>
    public decimal GetDecimal(int i) => _dataReader.GetDecimal(i);

    /// <summary>
    /// Gets the date and time data value of the specified field.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The date and time data value of the specified field.
    /// </returns>
    public DateTime GetDateTime(int i) => _dataReader.GetDateTime(i);

    /// <summary>
    /// Returns an <see cref="T:System.Data.IDataReader"></see> for the specified column ordinal.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// The <see cref="T:System.Data.IDataReader"></see> for the specified column ordinal.
    /// </returns>
    public IDataReader GetData(int i) => _dataReader.GetData(i);

    /// <summary>
    /// Return whether the specified field is set to null.
    /// </summary>
    /// <param name="i">The index of the field to find.</param>
    /// <returns>
    /// true if the specified field is set to null; otherwise, false.
    /// </returns>
    public bool IsDBNull(int i) => _dataReader.IsDBNull(i);

    /// <summary>
    /// Gets the number of columns in the current row.
    /// </summary>
    public int FieldCount => _dataReader.FieldCount;

    /// <summary>
    /// Gets the <see cref="System.Object"/> with the specified i.
    /// </summary>
    /// <value>
    /// The <see cref="System.Object"/>.
    /// </value>
    /// <param name="i">The i.</param>
    /// <returns></returns>
    object IDataRecord.this[int i] => _dataReader[i];

    /// <summary>
    /// Gets the <see cref="System.Object"/> with the specified name.
    /// </summary>
    /// <value>
    /// The <see cref="System.Object"/>.
    /// </value>
    /// <param name="name">The name.</param>
    /// <returns></returns>
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
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() => _dataReader.Dispose();

    /// <summary>
    /// Closes the <see cref="T:System.Data.IDataReader"></see> Object.
    /// </summary>
    public void Close() => _dataReader.Dispose();

    /// <summary>
    /// Returns a <see cref="T:System.Data.DataTable"></see> that describes the column metadata of the <see cref="T:System.Data.IDataReader"></see>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Data.DataTable"></see> that describes the column metadata.
    /// </returns>
    public DataTable GetSchemaTable() => _dataReader.GetSchemaTable();

    /// <summary>
    /// Advances the data reader to the next result, when reading the results of batch SQL statements.
    /// </summary>
    /// <returns>
    /// true if there are more rows; otherwise, false.
    /// </returns>
    public bool NextResult() => _dataReader.NextResult();

    /// <summary>
    /// Advances the <see cref="T:System.Data.IDataReader"></see> to the next record.
    /// </summary>
    /// <returns>
    /// true if there are more rows; otherwise, false.
    /// </returns>
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