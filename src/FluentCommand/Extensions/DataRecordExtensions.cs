using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace FluentCommand.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IDataRecord"/>.
    /// </summary>
    public static class DataRecordExtensions
    {
        /// <summary>Gets the value of the specified column as a <see cref="Boolean"/>.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The value of the column.</returns>
        /// <remarks>Returns <see langword="false"/> for <see langword="null"/>.</remarks>
        public static bool GetBoolean(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return !dataRecord.IsDBNull(ordinal) && dataRecord.GetBoolean(ordinal);
        }

        /// <summary>Gets the value of the specified column as a <see cref="Boolean"/>.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The value of the column.</returns>
        /// <remarks>Returns <see langword="false"/> for <see langword="null"/>.</remarks>
        public static bool? GetBooleanNull(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return !dataRecord.IsDBNull(ordinal) ? (bool?)null : dataRecord.GetBoolean(ordinal);
        }


        /// <summary>Gets the 8-bit unsigned integer value of the specified column.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The 8-bit unsigned integer value of the specified column.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static byte GetByte(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (byte)0 : dataRecord.GetByte(ordinal);
        }

        /// <summary>Gets the 8-bit unsigned integer value of the specified column.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The 8-bit unsigned integer value of the specified column.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static byte? GetByteNull(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (byte?)null : dataRecord.GetByte(ordinal);
        }

        /// <summary>Gets a stream of bytes from the  specified column.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>A stream of bytes of the specified column.</returns>
        /// <remarks>Returns empty array for <see langword="null"/>.</remarks>
        public static byte[] GetBytes(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            if (dataRecord.IsDBNull(ordinal))
                return new byte[0];

            //get the length of data
            long size = dataRecord.GetBytes(ordinal, 0, null, 0, 0);  
            byte[] buffer = new byte[size];

            int bufferSize = 1024;
            long bytesRead = 0;
            int offset = 0;

            while (bytesRead < size)
            {
                bytesRead += dataRecord.GetBytes(ordinal, offset, buffer, offset, bufferSize);
                offset += bufferSize;
            }

            return buffer;
        }


        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <param name="fieldOffset">The index within the field from which to start the read operation.</param>
        /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
        /// <param name="bufferOffset">The index for buffer to start the read operation.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The actual number of bytes read.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static Int64 GetBytes(this IDataRecord dataRecord, string name, Int64 fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? 0 : dataRecord.GetBytes(dataRecord.GetOrdinal(name), fieldOffset, buffer, bufferOffset, length);
        }

        /// <summary>Gets the character value of the specified column.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The character value of the specified column.</returns>
        /// <remarks>Returns Char.MinValue for <see langword="null"/>.</remarks>
        public static char GetChar(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? Char.MinValue : dataRecord.GetChar(ordinal);
        }

        /// <summary>Gets the character value of the specified column.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The character value of the specified column.</returns>
        /// <remarks>Returns Char.MinValue for <see langword="null"/>.</remarks>
        public static char? GetCharNull(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (Char?)null : dataRecord.GetChar(ordinal);
        }

        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <param name="fieldOffset">The index within the row from which to start the read operation.</param>
        /// <param name="buffer">The buffer into which to read the stream of bytes.</param>
        /// <param name="bufferOffset">The index for buffer to start the read operation. </param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The actual number of characters read.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static Int64 GetChars(this IDataRecord dataRecord, string name, Int64 fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? 0 : dataRecord.GetChars(dataRecord.GetOrdinal(name), fieldOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// Returns an <see cref="IDataReader"/> for the specified column <paramref name="name"/>.
        /// </summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The <see cref="IDataReader"/> for the specified column <paramref name="name"/>.</returns>
        public static IDataReader GetData(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? null : dataRecord.GetData(ordinal);
        }

        /// <summary>Gets the data type information for the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The data type information for the specified field.</returns>
        public static string GetDataTypeName(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.GetDataTypeName(ordinal);
        }

        /// <summary>Gets the date and time data value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The date and time data value of the specified field.</returns>
        /// <remarks>Returns DateTime.MinValue for <see langword="null"/>.</remarks>
        public static DateTime GetDateTime(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? DateTime.MinValue : dataRecord.GetDateTime(ordinal);
        }

        /// <summary>Gets the date and time data value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The date and time data value of the specified field.</returns>
        /// <remarks>Returns DateTime.MinValue for <see langword="null"/>.</remarks>
        public static DateTime? GetDateTimeNull(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (DateTime?)null : dataRecord.GetDateTime(ordinal);
        }

        /// <summary>Gets the date and time data value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The date and time data value of the specified field.</returns>
        /// <remarks>Returns DateTime.MinValue for <see langword="null"/>.</remarks>
        public static DateTimeOffset GetDateTimeOffset(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            if (dataRecord.IsDBNull(ordinal))
                return DateTimeOffset.MinValue;

            var value = dataRecord.GetValue(ordinal);
            if (value is DateTimeOffset offset)
                return offset;

            var date = dataRecord.GetDateTime(ordinal);
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            return new DateTimeOffset(date, TimeSpan.Zero);
        }

        /// <summary>Gets the date and time data value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The date and time data value of the specified field.</returns>
        /// <remarks>Returns DateTime.MinValue for <see langword="null"/>.</remarks>
        public static DateTimeOffset? GetDateTimeOffsetNull(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            if (dataRecord.IsDBNull(ordinal))
                return null;

            var value = dataRecord.GetValue(ordinal);
            if (value is DateTimeOffset offset)
                return offset;

            var date = dataRecord.GetDateTime(ordinal);
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            return new DateTimeOffset(date, TimeSpan.Zero);
        }

        /// <summary>Gets the fixed-position numeric value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The fixed-position numeric value of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static decimal GetDecimal(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? 0 : dataRecord.GetDecimal(ordinal);
        }

        /// <summary>Gets the fixed-position numeric value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The fixed-position numeric value of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static decimal? GetDecimalNull(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (decimal?)null : dataRecord.GetDecimal(ordinal);
        }

        /// <summary>Gets the double-precision floating point number of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The double-precision floating point number of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static double GetDouble(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? 0D : dataRecord.GetDouble(ordinal);
        }

        /// <summary>Gets the double-precision floating point number of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The double-precision floating point number of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static double? GetDoubleNull(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (double?)null : dataRecord.GetDouble(ordinal);
        }

        /// <summary>
        /// Gets the <see cref="Type"/> information corresponding to the type of <see cref="Object"/> that would be returned from <see cref="GetValue"/>.
        /// </summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The <see cref="Type"/> information corresponding to the type of <see cref="Object"/> that would be returned from <see cref="GetValue"/>.</returns>
        public static Type GetFieldType(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.GetFieldType(ordinal);
        }

        /// <summary>Gets the single-precision floating point number of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The single-precision floating point number of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static float GetFloat(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? 0F : dataRecord.GetFloat(ordinal);
        }

        /// <summary>Gets the single-precision floating point number of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The single-precision floating point number of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static float? GetFloatNull(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (float?)null : dataRecord.GetFloat(ordinal);
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> value of the specified field..
        /// </summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The <see cref="Guid"/> value of the specified field.</returns>
        /// <remarks>Returns Guid.Empty for <see langword="null"/>.</remarks>
        public static Guid GetGuid(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? Guid.Empty : dataRecord.GetGuid(ordinal);
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> value of the specified field..
        /// </summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The <see cref="Guid"/> value of the specified field.</returns>
        /// <remarks>Returns Guid.Empty for <see langword="null"/>.</remarks>
        public static Guid? GetGuidNull(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (Guid?)null : dataRecord.GetGuid(ordinal);
        }

        /// <summary>Gets the 16-bit signed integer value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The 16-bit signed integer value of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static Int16 GetInt16(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (Int16)0 : dataRecord.GetInt16(ordinal);
        }

        /// <summary>Gets the 16-bit signed integer value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The 16-bit signed integer value of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static Int16? GetInt16Null(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (Int16?)null : dataRecord.GetInt16(ordinal);
        }

        /// <summary>Gets the 32-bit signed integer value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The 32-bit signed integer value of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static int GetInt32(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? 0 : dataRecord.GetInt32(ordinal);
        }

        /// <summary>Gets the 32-bit signed integer value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The 32-bit signed integer value of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static int? GetInt32Null(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (int?)null : dataRecord.GetInt32(ordinal);
        }

        /// <summary>Gets the 64-bit signed integer value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The 64-bit signed integer value of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static Int64 GetInt64(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? 0 : dataRecord.GetInt64(ordinal);
        }

        /// <summary>Gets the 64-bit signed integer value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The 64-bit signed integer value of the specified field.</returns>
        /// <remarks>Returns 0 for <see langword="null"/>.</remarks>
        public static Int64? GetInt64Null(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? (long?)null : dataRecord.GetInt64(ordinal);
        }

        /// <summary>Gets the string value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The string value of the specified field.</returns>
        /// <remarks>Returns String.Empty for <see langword="null"/>.</remarks>
        public static string GetString(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? string.Empty : dataRecord.GetString(ordinal);
        }

        /// <summary>Gets the string value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The string value of the specified field.</returns>
        public static string GetStringNull(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? null : dataRecord.GetString(ordinal);
        }

        /// <summary>Gets the value of the specified field.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns>The <see cref="Object"/> which will contain the field value upon return.</returns>
        public static object GetValue(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal) ? null : dataRecord.GetValue(ordinal);
        }

        /// <summary>Determines whether the specified field is set to <see langword="null"/>.</summary>
        /// <param name="dataRecord">The data record.</param>
        /// <param name="name">The <paramref name="name"/> of the field to find.</param>
        /// <returns><c>true</c> if the specified field is set to <see langword="null"/>; otherwise, <c>false</c>.</returns>
        public static bool IsDBNull(this IDataRecord dataRecord, string name)
        {
            int ordinal = dataRecord.GetOrdinal(name);
            return dataRecord.IsDBNull(ordinal);
        }
    }
}
