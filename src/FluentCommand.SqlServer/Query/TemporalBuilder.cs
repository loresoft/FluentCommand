using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL Server temporal table queries using <c>FOR SYSTEM_TIME</c> clauses.
/// </summary>
public class TemporalBuilder : StatementBuilder<TemporalBuilder>
{
    private TableExpression _from;
    private string _temporal;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemporalBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator used to build SQL expressions.</param>
    /// <param name="parameters">The list of query parameters.</param>
    public TemporalBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Specifies the source table for the temporal query using the table name, schema, and optional alias.
    /// </summary>
    /// <param name="tableName">The name of the table to query.</param>
    /// <param name="tableSchema">The schema of the table (optional).</param>
    /// <param name="tableAlias">The alias to use for the table in the query (optional).</param>
    /// <returns>The same <see cref="TemporalBuilder"/> instance for fluent chaining.</returns>
    public TemporalBuilder From(
        string tableName,
        string tableSchema = null,
        string tableAlias = null)
    {
        _from = new TableExpression(tableName, tableSchema, tableAlias);

        return this;
    }

    /// <summary>
    /// Specifies the source table for the temporal query using the entity type and optional alias.
    /// </summary>
    /// <typeparam name="TEntity">The entity type representing the table.</typeparam>
    /// <param name="tableAlias">The alias to use for the table in the query (optional).</param>
    /// <returns>The same <see cref="TemporalBuilder"/> instance for fluent chaining.</returns>
    public TemporalBuilder From<TEntity>(
        string tableAlias = null)
    {
        var typeAccessor = TypeAccessor.GetAccessor<TEntity>();

        _from = new TableExpression(typeAccessor.TableName, typeAccessor.TableSchema, tableAlias);

        return this;
    }

    /// <summary>
    /// Configures the query to return all rows from the temporal table's history using <c>FOR SYSTEM_TIME ALL</c>.
    /// </summary>
    /// <returns>The same <see cref="TemporalBuilder"/> instance for fluent chaining.</returns>
    public TemporalBuilder All()
    {
        _temporal = "FOR SYSTEM_TIME ALL";
        return this;
    }

    /// <summary>
    /// Configures the query to return rows as of a specific point in time using <c>FOR SYSTEM_TIME AS OF</c>.
    /// </summary>
    /// <param name="utcPointInTime">The UTC date and time to query as of.</param>
    /// <returns>The same <see cref="TemporalBuilder"/> instance for fluent chaining.</returns>
    public TemporalBuilder AsOf(DateTime utcPointInTime)
    {
        var paramName = NextParameter();
        var queryParameter = new QueryParameter(paramName, utcPointInTime, typeof(DateTime));

        _temporal = $"FOR SYSTEM_TIME AS OF {paramName}";
        Parameters.Add(queryParameter);

        return this;
    }

    /// <summary>
    /// Configures the query to return rows that were active between two points in time using <c>FOR SYSTEM_TIME BETWEEN</c>.
    /// </summary>
    /// <param name="utcStart">The UTC start date and time.</param>
    /// <param name="utcEnd">The UTC end date and time.</param>
    /// <returns>The same <see cref="TemporalBuilder"/> instance for fluent chaining.</returns>
    public TemporalBuilder Between(DateTime utcStart, DateTime utcEnd)
    {
        var nameStart = NextParameter();
        var paramStart = new QueryParameter(nameStart, utcStart, typeof(DateTime));

        var nameEnd = NextParameter();
        var paramEnd = new QueryParameter(nameEnd, utcEnd, typeof(DateTime));

        _temporal = $"FOR SYSTEM_TIME BETWEEN {nameStart} AND {nameEnd}";

        Parameters.Add(paramStart);
        Parameters.Add(paramEnd);

        return this;
    }

    /// <summary>
    /// Configures the query to return rows whose period is contained within the specified range using <c>FOR SYSTEM_TIME CONTAINED IN</c>.
    /// </summary>
    /// <param name="utcStart">The UTC start date and time.</param>
    /// <param name="utcEnd">The UTC end date and time.</param>
    /// <returns>The same <see cref="TemporalBuilder"/> instance for fluent chaining.</returns>
    public TemporalBuilder ContainedIn(DateTime utcStart, DateTime utcEnd)
    {
        var nameStart = NextParameter();
        var paramStart = new QueryParameter(nameStart, utcStart, typeof(DateTime));

        var nameEnd = NextParameter();
        var paramEnd = new QueryParameter(nameEnd, utcEnd, typeof(DateTime));

        _temporal = $"FOR SYSTEM_TIME CONTAINED IN ({nameStart}, {nameEnd})";

        Parameters.Add(paramStart);
        Parameters.Add(paramEnd);

        return this;
    }

    /// <summary>
    /// Configures the query to return rows that were active from the start time to the end time using <c>FOR SYSTEM_TIME FROM ... AND ...</c>.
    /// </summary>
    /// <param name="utcStart">The UTC start date and time.</param>
    /// <param name="utcEnd">The UTC end date and time.</param>
    /// <returns>The same <see cref="TemporalBuilder"/> instance for fluent chaining.</returns>
    public TemporalBuilder FromTo(DateTime utcStart, DateTime utcEnd)
    {
        var nameStart = NextParameter();
        var paramStart = new QueryParameter(nameStart, utcStart, typeof(DateTime));

        var nameEnd = NextParameter();
        var paramEnd = new QueryParameter(nameEnd, utcEnd, typeof(DateTime));

        _temporal = $"FOR SYSTEM_TIME FROM {nameStart} AND {nameEnd}";

        Parameters.Add(paramStart);
        Parameters.Add(paramEnd);

        return this;
    }

    /// <summary>
    /// Builds the SQL statement for the temporal query using the specified table and temporal clause.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> representing the constructed temporal query and its parameters.
    /// </returns>
    public override QueryStatement BuildStatement()
    {
        if (_temporal.IsNullOrWhiteSpace())
            return new QueryStatement(QueryGenerator.TableExpression(_from), Parameters);

        var table = QueryGenerator.TableExpression(new TableExpression(_from.TableName, _from.TableSchema));

        var statement = $"{table} {_temporal}";

        if (_from.TableAlias.HasValue())
            statement += $" AS [{_from.TableAlias}]";

        return new QueryStatement(statement, Parameters);
    }
}
