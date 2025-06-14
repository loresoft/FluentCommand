using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL Server <c>CHANGETABLE (CHANGES ...)</c> queries for change tracking.
/// </summary>
public class ChangeTableBuilder : StatementBuilder<ChangeTableBuilder>
{
    private TableExpression _fromTable;
    private QueryParameter _parameter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeTableBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator used to build SQL expressions.</param>
    /// <param name="parameters">The list of query parameters.</param>
    public ChangeTableBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters) : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Specifies the source table for the <c>CHANGETABLE</c> query using the table name, schema, and optional alias.
    /// </summary>
    /// <param name="tableName">The name of the table to track changes for.</param>
    /// <param name="tableSchema">The schema of the table (optional).</param>
    /// <param name="tableAlias">The alias to use for the table in the query (optional).</param>
    /// <returns>The same <see cref="ChangeTableBuilder"/> instance for fluent chaining.</returns>
    public ChangeTableBuilder From(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        _fromTable = new TableExpression(tableName, tableSchema, tableAlias);

        return this;
    }

    /// <summary>
    /// Specifies the source table for the <c>CHANGETABLE</c> query using the entity type and optional alias.
    /// </summary>
    /// <typeparam name="TEntity">The entity type representing the table.</typeparam>
    /// <param name="tableAlias">The alias to use for the table in the query (optional).</param>
    /// <returns>The same <see cref="ChangeTableBuilder"/> instance for fluent chaining.</returns>
    public ChangeTableBuilder From<TEntity>(
        string tableAlias = null)
    {
        var typeAccessor = TypeAccessor.GetAccessor<TEntity>();

        _fromTable = new TableExpression(typeAccessor.TableName, typeAccessor.TableSchema, tableAlias);

        return this;
    }

    /// <summary>
    /// Sets the last version value for the <c>CHANGETABLE</c> query, which is used to retrieve changes since the specified version.
    /// </summary>
    /// <param name="lastVersion">The last version number to use for change tracking.</param>
    /// <returns>The same <see cref="ChangeTableBuilder"/> instance for fluent chaining.</returns>
    public ChangeTableBuilder LastVersion(long lastVersion)
    {
        var name = NextParameter();
        _parameter = new QueryParameter(name, lastVersion, typeof(long));

        Parameters.Add(_parameter);

        return this;
    }

    /// <summary>
    /// Builds the <c>CHANGETABLE (CHANGES ...)</c> SQL statement using the specified table and version.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> representing the constructed <c>CHANGETABLE</c> query and its parameters.
    /// </returns>
    public override QueryStatement BuildStatement()
    {
        if (_parameter == null)
            return new QueryStatement(QueryGenerator.TableExpression(_fromTable), Parameters);

        var table = QueryGenerator.TableExpression(new TableExpression(_fromTable.TableName, _fromTable.TableSchema));

        var statement = $"CHANGETABLE (CHANGES {table}, {_parameter.Name})";

        if (_fromTable.TableAlias.HasValue())
            statement += $" AS [{_fromTable.TableAlias}]";

        return new QueryStatement(statement, Parameters);
    }
}
