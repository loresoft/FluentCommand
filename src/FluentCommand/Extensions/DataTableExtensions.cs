using System.Data;

namespace FluentCommand.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="DataTable"/> class.
/// </summary>
public static class DataTableExtensions
{
    /// <summary>
    /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="DataTable"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source collection.</typeparam>
    /// <param name="source">The collection of objects to convert to a <see cref="DataTable"/>.</param>
    /// <param name="ignoreNames">
    /// An optional collection of property names to ignore when creating columns in the <see cref="DataTable"/>.
    /// If <c>null</c>, all public properties are included.
    /// </param>
    /// <returns>
    /// A <see cref="DataTable"/> populated with the data from the <paramref name="source"/> collection,
    /// or <c>null</c> if <paramref name="source"/> is <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This method uses <see cref="ListDataReader{T}"/> to read the data from the source collection and load it into a <see cref="DataTable"/>.
    /// </remarks>
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
