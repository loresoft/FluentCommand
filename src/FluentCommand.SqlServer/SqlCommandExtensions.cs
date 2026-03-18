using System.Data;
using System.Data.Common;

using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;

namespace FluentCommand;

/// <summary>
/// Extension methods for <see cref="IDataCommand"/> specific to SQL Server.
/// </summary>
public static class SqlCommandExtensions
{
    /// <summary>
    /// Adds a new SQL Server structured table-valued parameter with the specified <paramref name="name"/> and <paramref name="data"/>.
    /// Uses <see cref="SqlDataRecordAdapter{T}"/> internally for maximum efficiency by reusing a single record per row
    /// with cached metadata per type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the data entities.</typeparam>
    /// <param name="dataCommand">The <see cref="IDataCommand"/> to extend.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="data">The enumerable data to be added as a table-valued parameter.</param>
    /// <returns>
    /// The same <see cref="IDataCommand"/> instance for fluent chaining.
    /// </returns>
    public static IDataCommand ParameterStructured<TEntity>(this IDataCommand dataCommand, string name, IEnumerable<TEntity> data)
        where TEntity : class
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        if (data is null)
            throw new ArgumentNullException(nameof(data));

        var records = new SqlDataRecordAdapter<TEntity>(data);
        return ParameterStructured(dataCommand, name, records);
    }

    /// <summary>
    /// Adds a new SQL Server structured table-valued parameter with the specified <paramref name="name"/> and <paramref name="records"/>.
    /// Uses <see cref="IEnumerable{SqlDataRecord}"/> for maximum efficiency with minimal memory allocation.
    /// </summary>
    /// <param name="dataCommand">The <see cref="IDataCommand"/> to extend.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="records">The <see cref="IEnumerable{SqlDataRecord}"/> to be added as a table-valued parameter.</param>
    /// <returns>
    /// The same <see cref="IDataCommand"/> instance for fluent chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the underlying command is not a SQL Server command.
    /// </exception>
    public static IDataCommand ParameterStructured(this IDataCommand dataCommand, string name, IEnumerable<SqlDataRecord> records)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        if (records is null)
            throw new ArgumentNullException(nameof(records));

        var sqlParameter = CreateSqlParameter(dataCommand);

        sqlParameter.ParameterName = name;
        sqlParameter.Value = records;
        sqlParameter.Direction = ParameterDirection.Input;
        sqlParameter.SqlDbType = SqlDbType.Structured;

        return dataCommand.Parameter(sqlParameter);
    }

    /// <summary>
    /// Adds a new SQL Server structured table-valued parameter with the specified <paramref name="name"/> and <paramref name="dataReader"/>.
    /// Uses <see cref="DbDataReader"/> for streaming data to the server without materializing to a <see cref="DataTable"/>.
    /// </summary>
    /// <param name="dataCommand">The <see cref="IDataCommand"/> to extend.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="dataReader">The <see cref="DbDataReader"/> to be added as a table-valued parameter.</param>
    /// <returns>
    /// The same <see cref="IDataCommand"/> instance for fluent chaining.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the underlying command is not a SQL Server command.
    /// </exception>
    public static IDataCommand ParameterStructured(this IDataCommand dataCommand, string name, DbDataReader dataReader)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        if (dataReader is null)
            throw new ArgumentNullException(nameof(dataReader));

        var sqlParameter = CreateSqlParameter(dataCommand);

        sqlParameter.ParameterName = name;
        sqlParameter.Value = dataReader;
        sqlParameter.Direction = ParameterDirection.Input;
        sqlParameter.SqlDbType = SqlDbType.Structured;

        return dataCommand.Parameter(sqlParameter);
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
    public static IDataCommand ParameterStructured(this IDataCommand dataCommand, string name, DataTable dataTable)
    {
        if (dataCommand is null)
            throw new ArgumentNullException(nameof(dataCommand));

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

        if (dataTable is null)
            throw new ArgumentNullException(nameof(dataTable));

        var sqlParameter = CreateSqlParameter(dataCommand);

        sqlParameter.ParameterName = name;
        sqlParameter.Value = dataTable;
        sqlParameter.Direction = ParameterDirection.Input;
        sqlParameter.SqlDbType = SqlDbType.Structured;

        return dataCommand.Parameter(sqlParameter);
    }

    private static SqlParameter CreateSqlParameter(IDataCommand dataCommand)
    {
        var parameter = dataCommand.Command.CreateParameter();
        var sqlParameter = parameter as SqlParameter;
        if (sqlParameter == null)
            throw new InvalidOperationException(
                "SqlParameter only supported by SQL Server.  Make sure DataSession was created with a valid SqlConnection.");

        return sqlParameter;
    }
}
