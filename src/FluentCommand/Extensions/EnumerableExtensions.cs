using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;

namespace FluentCommand.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="T:System.Collection.IEnumerable"/>
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Converts an IEnumerable of values to a delimited string.
        /// </summary>
        /// <typeparam name="T">
        /// The type of objects to delimit.
        /// </typeparam>
        /// <param name="values">
        /// The IEnumerable string values to convert.
        /// </param>
        /// <returns>
        /// A delimited string of the values.
        /// </returns>
        public static string ToDelimitedString<T>(this IEnumerable<T> values)
        {
            return ToDelimitedString<T>(values, ",");
        }

        /// <summary>
        /// Converts an IEnumerable of values to a delimited string.
        /// </summary>
        /// <typeparam name="T">
        /// The type of objects to delimit.
        /// </typeparam>
        /// <param name="values">
        /// The IEnumerable string values to convert.
        /// </param>
        /// <param name="delimiter">
        /// The delimiter.
        /// </param>
        /// <returns>
        /// A delimited string of the values.
        /// </returns>
        public static string ToDelimitedString<T>(this IEnumerable<T> values, string delimiter)
        {
            var sb = new StringBuilder();
            foreach (var i in values)
            {
                if (sb.Length > 0)
                    sb.Append(delimiter ?? ",");
                sb.Append(i.ToString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts an IEnumerable of values to a delimited string.
        /// </summary>
        /// <param name="values">The IEnumerable string values to convert.</param>
        /// <returns>A delimited string of the values.</returns>
        public static string ToDelimitedString(this IEnumerable<string> values)
        {
            return ToDelimitedString(values, ",");
        }

        /// <summary>
        /// Converts an IEnumerable of values to a delimited string.
        /// </summary>
        /// <param name="values">The IEnumerable string values to convert.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>A delimited string of the values.</returns>
        public static string ToDelimitedString(this IEnumerable<string> values, string delimiter)
        {
            return ToDelimitedString(values, delimiter, null);
        }

        /// <summary>
        /// Converts an IEnumerable of values to a delimited string.
        /// </summary>
        /// <param name="values">The IEnumerable string values to convert.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="escapeDelimiter">A delegate used to escape the delimiter contained in the value.</param>
        /// <returns>A delimited string of the values.</returns>
        public static string ToDelimitedString(this IEnumerable<string> values, string delimiter, Func<string, string> escapeDelimiter)
        {
            var sb = new StringBuilder();
            foreach (var value in values)
            {
                if (sb.Length > 0)
                    sb.Append(delimiter);

                var v = escapeDelimiter != null
                    ? escapeDelimiter(value ?? string.Empty)
                    : value ?? string.Empty;

                sb.Append(v);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a <see cref="T:System.Collections.Generic.HashSet`1"/> from an <see cref="T:System.Collections.Generic.IEnumerable`1"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1"/> to create a <see cref="T:System.Collections.Generic.HashSet`1"/> from.</param>
        /// <returns>A <see cref="T:System.Collections.Generic.HashSet`1"/> that contains elements from the input sequence.</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        /// <summary>
        /// Creates a <see cref="T:System.Collections.Generic.HashSet`1"/> from an <see cref="T:System.Collections.Generic.IEnumerable`1"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1"/> to create a <see cref="T:System.Collections.Generic.HashSet`1"/> from.</param>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> to compare elements.</param>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.HashSet`1"/> that contains elements from the input sequence.
        /// </returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(source, comparer);
        }

        /// <summary>
        /// Converts the IEnumerable to a <see cref="DataTable"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source to convert.</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> source)
        {
            return ToDataTable<T>(source, null);
        }

        /// <summary>
        /// Converts the IEnumerable to a <see cref="DataTable" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source to convert.</param>
        /// <param name="ignoreNames">The ignored property names.</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> source, IEnumerable<string> ignoreNames)
        {
            return ToDataTable<T>(source, ignoreNames, true);
        }

        /// <summary>
        /// Converts the IEnumerable to a <see cref="DataTable" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source to convert.</param>
        /// <param name="ignoreNames">The ignored property names.</param>
        /// <param name="useAttribute"><c>true</c> to use the ColumnAttribute in mapping to the DataTable.</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> source, IEnumerable<string> ignoreNames, bool useAttribute)
        {
            if (source == null)
                return null;

            var entityType = typeof(T);
            var properties = TypeDescriptor.GetProperties(entityType);
            var ignored = new HashSet<string>(ignoreNames ?? Enumerable.Empty<string>());
            
            // Tuple<PropertyName, ColumnName, Type, Order>
            var columns = CreateColumns(properties, ignored, useAttribute).ToList();
            var table = CreateTable<T>(columns);

            // copy data
            foreach (T item in source)
            {
                DataRow row = table.NewRow();

                foreach (var c in columns)
                {
                    string propertyName = c.Item1;
                    string columnName = c.Item2;

                    var propertyDescriptor = properties[propertyName];
                    row[columnName] = propertyDescriptor.GetValue(item) ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }

        private static DataTable CreateTable<T>(IEnumerable<Tuple<string, string, Type, int>> columns)
        {
            var entityType = typeof(T);
            var table = new DataTable(entityType.Name);

            // sort columns
            // Tuple<PropertyName, ColumnName, Type, Order>
            foreach (var column in columns.OrderBy(c => c.Item4))
            {
                var dataColumn = new DataColumn();
                dataColumn.ColumnName = column.Item2;
                dataColumn.DataType = column.Item3;

                table.Columns.Add(dataColumn);
            }

            return table;
        }

        // Tuple<PropertyName, ColumnName, Type, Order>
        private static IEnumerable<Tuple<string, string, Type, int>> CreateColumns(PropertyDescriptorCollection properties, HashSet<string> ignored, bool useAttribute)
        {
            var columns = new List<Tuple<string, string, Type, int>>();

            foreach (PropertyDescriptor p in properties)
            {
                if (ignored.Contains(p.Name))
                    continue;

                string propertyName = p.Name;
                string columnName = p.Name;
                int order = columns.Count;
                Type type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

                if (useAttribute)
                {
                    var display = p.Attributes
                        .OfType<ColumnAttribute>()
                        .FirstOrDefault();

                    if (display != null && display.Name.HasValue())
                        columnName = display.Name;

                    if (display != null && display.Order > 0)
                        order = display.Order;
                }

                var column = new Tuple<string, string, Type, int>(propertyName, columnName, type, order);
                columns.Add(column);
            }
            return columns;
        }
    }

}
