using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using FluentCommand.Query.Generators;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL UPSERT statements with fluent, chainable methods.
/// </summary>
public class UpsertBuilder : UpsertBuilder<UpsertBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpsertBuilder"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    public UpsertBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    { }
}

/// <summary>
/// Provides a generic base class for building SQL UPSERT statements with fluent, chainable methods.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder for fluent chaining.</typeparam>
public abstract class UpsertBuilder<TBuilder> : StatementBuilder<TBuilder>
    where TBuilder : UpsertBuilder<TBuilder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpsertBuilder{TBuilder}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    protected UpsertBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Gets the collection of column expressions for the UPSERT statement.
    /// </summary>
    protected HashSet<ColumnExpression> ColumnExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of value expressions for the UPSERT statement.
    /// </summary>
    protected HashSet<string> ValueExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of key column expressions for the UPSERT statement.
    /// </summary>
    protected HashSet<ColumnExpression> KeyExpressions { get; } = new();

    /// <summary>
    /// Gets the collection of output column expressions for the UPSERT statement.
    /// </summary>
    protected HashSet<ColumnExpression> OutputExpressions { get; } = new();

    /// <summary>
    /// Gets the target table expression for the UPSERT statement.
    /// </summary>
    protected TableExpression? TableExpression { get; private set; }

    /// <summary>
    /// Sets the target table to insert into or update.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="tableSchema">The schema of the table (optional).</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public TBuilder Into(
        string tableName,
        string? tableSchema = null,
        string? tableAlias = null)
    {
        TableExpression = new TableExpression(tableName, tableSchema, tableAlias);

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a value for the specified column name and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValue">The value to insert or update for the column.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public TBuilder Value<TValue>(
        string columnName,
        TValue? parameterValue)
    {
        return Value(columnName, parameterValue, typeof(TValue));
    }

    /// <summary>
    /// Adds a value for the specified column name, value, and type.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValue">The value to insert or update for the column.</param>
    /// <param name="parameterType">The type of the parameter value.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <remarks>
    /// Enum and nullable enum values are normalized to their numeric underlying type.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown if <paramref name="columnName"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parameterType"/> is <c>null</c>.</exception>
    public TBuilder Value(
        string columnName,
        object? parameterValue,
        Type parameterType)
    {
        if (string.IsNullOrWhiteSpace(columnName))
            throw new ArgumentException($"'{nameof(columnName)}' cannot be null or empty.", nameof(columnName));

        ArgumentNullException.ThrowIfNull(parameterType);

        var paramterName = NextParameter();

        var columnExpression = new ColumnExpression(columnName);

        ColumnExpressions.Add(columnExpression);
        ValueExpressions.Add(paramterName);

        var normalized = QueryParameterNormalizer.Normalize(parameterValue, parameterType);
        Parameters.Add(new QueryParameter(paramterName, normalized.Value, normalized.Type));

        return (TBuilder)this;
    }

    /// <summary>
    /// Conditionally adds a value for the specified column name and value if the condition is met.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValue">The value to insert or update for the column.</param>
    /// <param name="condition">A function that determines whether to add the value, based on the column name and value.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public TBuilder ValueIf<TValue>(
        string columnName,
        TValue? parameterValue,
        Func<string, TValue?, bool> condition)
    {
        if (condition != null && !condition(columnName, parameterValue))
            return (TBuilder)this;

        return Value(columnName, parameterValue);
    }

    /// <summary>
    /// Adds a value for the specified column name with the value serialized as JSON using the specified <paramref name="options" />.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValue">The value to serialize as JSON and insert or update for the column.</param>
    /// <param name="options">The <see cref="JsonSerializerOptions"/> to use when serializing.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public TBuilder ValueJson<TValue>(
        string columnName,
        TValue? parameterValue,
        JsonSerializerOptions? options = null)
    {
        var json = parameterValue is not null
            ? JsonSerializer.Serialize(parameterValue, options ?? JsonSerializerOptions)
            : null;

        return Value(columnName, json);
    }

    /// <summary>
    /// Adds a value for the specified column name with the value serialized as JSON using the specified <paramref name="jsonTypeInfo" />.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="parameterValue">The value to serialize as JSON and insert or update for the column.</param>
    /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo{T}"/> to use when serializing.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public TBuilder ValueJson<TValue>(
        string columnName,
        TValue? parameterValue,
        JsonTypeInfo<TValue> jsonTypeInfo)
    {
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);

        var json = parameterValue is not null
            ? JsonSerializer.Serialize(parameterValue, jsonTypeInfo)
            : null;

        return Value(columnName, json);
    }


    /// <summary>
    /// Adds a key column used to determine whether a row already exists.
    /// </summary>
    /// <param name="columnName">The key column name.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="columnName"/> is null or empty.</exception>
    public TBuilder Key(string columnName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        KeyExpressions.Add(new ColumnExpression(columnName));

        return (TBuilder)this;
    }

    /// <summary>
    /// Conditionally adds a key column used to determine whether a row already exists.
    /// </summary>
    /// <param name="columnName">The key column name.</param>
    /// <param name="condition">A function that determines whether to add the key column.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public TBuilder KeyIf(
        string columnName,
        Func<string, bool>? condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return Key(columnName);
    }

    /// <summary>
    /// Adds an OUTPUT clause for the specified column names.
    /// </summary>
    /// <param name="columnNames">The collection of column names to include in the OUTPUT clause.</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="columnNames"/> is <c>null</c>.</exception>
    public TBuilder Output(
        IEnumerable<string> columnNames,
        string? tableAlias = null)
    {
        ArgumentNullException.ThrowIfNull(columnNames);

        foreach (var column in columnNames)
            Output(column, tableAlias);

        return (TBuilder)this;
    }

    /// <summary>
    /// Adds an OUTPUT clause for the specified column name.
    /// </summary>
    /// <param name="columnName">The name of the column to include in the OUTPUT clause.</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <param name="columnAlias">The alias for the output column (optional).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public TBuilder Output(
        string columnName,
        string? tableAlias = null,
        string? columnAlias = null)
    {
        var outputClause = new ColumnExpression(columnName, tableAlias, columnAlias);

        OutputExpressions.Add(outputClause);

        return (TBuilder)this;
    }

    /// <summary>
    /// Conditionally adds an OUTPUT clause for the specified column name if the condition is met.
    /// </summary>
    /// <param name="columnName">The name of the column to include in the OUTPUT clause.</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <param name="columnAlias">The alias for the output column (optional).</param>
    /// <param name="condition">A function that determines whether to add the OUTPUT clause.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public TBuilder OutputIf(
        string columnName,
        string? tableAlias = null,
        string? columnAlias = null,
        Func<string, bool>? condition = null)
    {
        if (condition != null && !condition(columnName))
            return (TBuilder)this;

        return Output(columnName, tableAlias, columnAlias);
    }

    /// <summary>
    /// Builds the SQL UPSERT statement using the current configuration.
    /// </summary>
    /// <returns>A <see cref="QueryStatement"/> containing the SQL UPSERT statement and its parameters.</returns>
    public override QueryStatement? BuildStatement()
    {
        if (TableExpression is null)
            throw new InvalidOperationException("Table must be specified before building an upsert statement.");

        if (ValueExpressions.Count == 0)
            throw new InvalidOperationException("Values must be specified before building an upsert statement.");

        if (KeyExpressions.Count == 0)
            throw new InvalidOperationException("Keys must be specified before building an upsert statement.");

        var updateExpressions = BuildUpdateExpressions();
        if (updateExpressions.Count == 0)
            throw new InvalidOperationException("At least one non-key value must be specified before building an upsert statement.");

        var upsertStatement = new UpsertStatement(
            TableExpression,
            ColumnExpressions,
            ValueExpressions,
            KeyExpressions,
            updateExpressions,
            OutputExpressions,
            CommentExpressions);

        var statement = QueryGenerator.BuildUpsert(upsertStatement);

        return new QueryStatement(statement, Parameters);
    }

    private IReadOnlyCollection<UpdateExpression> BuildUpdateExpressions()
    {
        var keyColumns = new HashSet<string>(KeyExpressions.Select(static k => k.ColumnName), StringComparer.OrdinalIgnoreCase);

        return ColumnExpressions
            .Where(c => !keyColumns.Contains(c.ColumnName))
            .Select(c => new UpdateExpression(c.ColumnName, string.Empty, c.TableAlias, c.IsRaw))
            .ToArray();
    }
}
