using System.Data;

using FluentCommand.Extensions;

using Microsoft.Data.SqlClient;

namespace FluentCommand;

/// <summary>
/// Extension methods for <see cref="IDataCommand"/> specific to SQL Server.
/// </summary>
public static class SqlCommandExtensions
{
    /// <summary>
    /// Adds a new SQL Server structured table-valued parameter with the specified <paramref name="name"/> and <paramref name="data"/>.
    /// Converts the enumerable data to a <see cref="DataTable"/> and adds it as a parameter.
    /// </summary>
    /// <typeparam name="TEntity">The type of the data entities.</typeparam>
    /// <param name="dataCommand">The <see cref="IDataCommand"/> to extend.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="data">The enumerable data to be added as a table-valued parameter.</param>
    /// <returns>
    /// The same <see cref="IDataCommand"/> instance for fluent chaining.
    /// </returns>
    public static IDataCommand SqlParameter<TEntity>(this IDataCommand dataCommand, string name, IEnumerable<TEntity> data)
        where TEntity : class
    {
        var dataTable = data.ToDataTable();
        return SqlParameter(dataCommand, name, dataTable);
    }

    /// <summary>
    /// Adds a new SQL Server structured table-valued parameter with the specified <paramref name="name"/> and <paramref name="dataTable"/>.
    /// </summary>
    /// <param name="dataCommand">The <see cref="IDataCommand"/> to extend.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="dataTable">The <see cref="DataTable"/> to be added as a table-valued parameter.</param>
    /// <returns>
    /// The same <see cref="IDataCommand"/> instance for fluent chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the underlying command is not a SQL Server command.
    /// </exception>
    public static IDataCommand SqlParameter(this IDataCommand dataCommand, string name, DataTable dataTable)
    {
        var parameter = dataCommand.Command.CreateParameter();
        var sqlParameter = parameter as SqlParameter;
        if (sqlParameter == null)
            throw new InvalidOperationException(
                "SqlParameter only supported by SQL Server.  Make sure DataSession was created with a valid SqlConnection.");

        sqlParameter.ParameterName = name;
        sqlParameter.Value = dataTable;
        sqlParameter.Direction = ParameterDirection.Input;
        sqlParameter.SqlDbType = SqlDbType.Structured;

        return dataCommand.Parameter(sqlParameter);
    }
}
