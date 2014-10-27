using System;
using System.Data;

namespace FluentCommand.Merge
{
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

        public string GetName(int i)
        {
            return _dataReader.GetName(i);
        }

        public string GetDataTypeName(int i)
        {
            return _dataReader.GetDataTypeName(i);
        }

        public Type GetFieldType(int i)
        {
            return _dataReader.GetFieldType(i);
        }

        public object GetValue(int i)
        {
            return _dataReader.GetValue(i);
        }

        public int GetValues(object[] values)
        {
            return _dataReader.GetValues(values);
        }

        public int GetOrdinal(string name)
        {
            string prefixName = _fieldPrefix != null
                ? _fieldPrefix + name
                : name;

            return _dataReader.GetOrdinal(prefixName);
        }

        public bool GetBoolean(int i)
        {
            return _dataReader.GetBoolean(i);
        }

        public byte GetByte(int i)
        {
            return _dataReader.GetByte(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return _dataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        public char GetChar(int i)
        {
            return _dataReader.GetChar(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return _dataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        public Guid GetGuid(int i)
        {
            return _dataReader.GetGuid(i);
        }

        public short GetInt16(int i)
        {
            return _dataReader.GetInt16(i);
        }

        public int GetInt32(int i)
        {
            return _dataReader.GetInt32(i);
        }

        public long GetInt64(int i)
        {
            return _dataReader.GetInt64(i);
        }

        public float GetFloat(int i)
        {
            return _dataReader.GetFloat(i);
        }

        public double GetDouble(int i)
        {
            return _dataReader.GetDouble(i);
        }

        public string GetString(int i)
        {
            return _dataReader.GetString(i);
        }

        public decimal GetDecimal(int i)
        {
            return _dataReader.GetDecimal(i);
        }

        public DateTime GetDateTime(int i)
        {
            return _dataReader.GetDateTime(i);
        }

        public IDataReader GetData(int i)
        {
            return _dataReader.GetData(i);
        }

        public bool IsDBNull(int i)
        {
            return _dataReader.IsDBNull(i);
        }

        public int FieldCount
        {
            get
            {
                return _dataReader.FieldCount;

            }
        }

        object IDataRecord.this[int i]
        {
            get
            {
                return _dataReader[i];
            }
        }

        object IDataRecord.this[string name]
        {
            get
            {
                return _dataReader[name];
            }
        }

        public void Dispose()
        {
            _dataReader.Dispose();
        }

        public void Close()
        {
            _dataReader.Dispose();
        }

        public DataTable GetSchemaTable()
        {
            return _dataReader.GetSchemaTable();
        }

        public bool NextResult()
        {
            return _dataReader.NextResult();
        }

        public bool Read()
        {
            return _dataReader.Read();
        }

        public int Depth
        {
            get
            {
                return _dataReader.Depth;

            }
        }

        public bool IsClosed
        {
            get
            {
                return _dataReader.IsClosed;
            }
        }

        public int RecordsAffected
        {
            get
            {
                return _dataReader.RecordsAffected;
            }
        }
    }
}