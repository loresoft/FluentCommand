using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;

namespace FluentCommand.Extensions
{
    /// <summary>
    /// Extension method for <see cref="DataTable"/>
    /// </summary>
    public static class DataTableExtensions
    {
        private static readonly ConcurrentDictionary<Type, IEnumerable<ColumnMap>> _cacheStore = new ConcurrentDictionary<Type, IEnumerable<ColumnMap>>();


        /// <summary>
        /// Converts the IEnumerable to a <see cref="DataTable" />.
        /// </summary>
        /// <typeparam name="T">The type of the source data</typeparam>
        /// <param name="source">The source to convert.</param>
        /// <param name="ignoreNames">The ignored property names.</param>
        /// <returns>A <see cref="DataTable"/> from the specified source.</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> source, IEnumerable<string> ignoreNames = null)
        {
            if (source == null)
                return null;

            var entityType = typeof(T);
            var properties = TypeDescriptor.GetProperties(entityType);
            var ignored = new HashSet<string>(ignoreNames ?? Enumerable.Empty<string>());

            var columns = CreateColumns<T>(properties, ignored).ToList();
            var table = CreateTable<T>(columns);

            // copy data
            foreach (T item in source)
            {
                var row = table.NewRow();

                foreach (var c in columns)
                {
                    string propertyName = c.PropertyName;
                    string columnName = c.ColumnName;

                    var propertyDescriptor = properties[propertyName];
                    row[columnName] = propertyDescriptor.GetValue(item) ?? DBNull.Value;
                }

                table.Rows.Add(row);
            }

            return table;
        }


        private static DataTable CreateTable<T>(IEnumerable<ColumnMap> columns)
        {
            var entityType = typeof(T);
            var table = new DataTable(entityType.Name);

            // sort columns
            // Tuple<PropertyName, ColumnName, Type, Order>
            foreach (var column in columns.OrderBy(c => c.Order))
            {
                var dataColumn = new DataColumn();
                dataColumn.ColumnName = column.ColumnName;
                dataColumn.DataType = column.Type;

                table.Columns.Add(dataColumn);
            }

            return table;
        }

        private static IEnumerable<ColumnMap> CreateColumns<T>(PropertyDescriptorCollection properties, HashSet<string> ignored)
        {
            return _cacheStore.GetOrAdd(typeof(T), key =>
            {
                var columns = new List<ColumnMap>();

                foreach (PropertyDescriptor p in properties)
                {
                    if (ignored.Contains(p.Name))
                        continue;

                    string propertyName = p.Name;
                    string columnName = p.Name;
                    int order = columns.Count;
                    Type type = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

                    var display = p.Attributes
                        .OfType<ColumnAttribute>()
                        .FirstOrDefault();

                    if (display != null)
                    {
                        if (display.Name.HasValue())
                            columnName = display.Name;

                        if (display.Order > 0)
                            order = display.Order;
                    }

                    var column = new ColumnMap(propertyName, columnName, type, order);
                    columns.Add(column);
                }

                return columns;
            });
        }

        private class ColumnMap
        {
            public ColumnMap(string propertyName, string columnName, Type type, int order)
            {
                PropertyName = propertyName;
                ColumnName = columnName;
                Type = type;
                Order = order;
            }

            public string PropertyName { get; }
            public string ColumnName { get; }
            public Type Type { get; }
            public int Order { get; }
        }
    }
}
