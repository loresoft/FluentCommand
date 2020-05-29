using System;
using System.Collections.Generic;

namespace FluentCommand
{
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
            {typeof(Guid), "uniqueidentifier"}
        };

        public static string NativeType<T>()
        {
            return NativeType(typeof(T));
        }

        public static string NativeType(Type type)
        {
            _nativeType.TryGetValue(type, out var value);

            return value ?? "sql_variant";
        }
    }
}
