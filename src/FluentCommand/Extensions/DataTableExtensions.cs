using System.Collections.Generic;
using System.Data;

namespace FluentCommand.Extensions;

/// <summary>
/// Extension method for <see cref="DataTable"/>
/// </summary>
public static class DataTableExtensions
{
    /// <summary>
    /// Converts the IEnumerable to a <see cref="DataTable" />.
    /// </summary>
    /// <typeparam name="T">The type of the source data</typeparam>
    /// <param name="source">The source to convert.</param>
    /// <param name="ignoreNames">The ignored property names.</param>
    /// <returns>A <see cref="DataTable"/> from the specified source.</returns>
    public static DataTable ToDataTable<T>(this IEnumerable<T> source, IEnumerable<string> ignoreNames = null)
        where T : class
    {
        if (source == null)
            return null;

        using var dataReader = new ListDataReader<T>(source, ignoreNames);

        var dataTable = new DataTable();
        dataTable.Load(dataReader);

        return dataTable;
    }
}
