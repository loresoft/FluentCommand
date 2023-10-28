using System.Linq.Expressions;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Update query statement builder
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class UpdateEntityBuilder<TEntity>
    : UpdateBuilder<UpdateEntityBuilder<TEntity>>, IWhereEntityBuilder<TEntity, UpdateEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateEntityBuilder{TEntity}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="logicalOperator">The logical operator.</param>
    public UpdateEntityBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <summary>
    /// Adds a value with specified property and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public UpdateEntityBuilder<TEntity> Value<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Value(propertyAccessor?.Column, parameterValue);
    }

    /// <summary>
    /// Conditionally adds a value with specified property and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public UpdateEntityBuilder<TEntity> ValueIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        Func<string, TValue, bool> condition)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return ValueIf(propertyAccessor?.Column, parameterValue, condition);
    }

    /// <summary>
    /// Adds a values from the specified entity. If column names are passed in,
    /// only those that match an entity property name will be included.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="columnNames">The column names to include.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public UpdateEntityBuilder<TEntity> Values(
        TEntity entity,
        IEnumerable<string> columnNames = null)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var properties = _typeAccessor.GetProperties();
        var columnSet = new HashSet<string>(columnNames ?? Enumerable.Empty<string>());

        foreach (var property in properties)
        {
            if (columnSet.Count > 0 && !columnSet.Contains(property.Column))
                continue;

            if (property.IsNotMapped || property.IsDatabaseGenerated)
                continue;

            // include the type to prevent issues with null
            Value(property.Column, property.GetValue(entity), property.MemberType);
        }

        return this;
    }


    /// <summary>
    /// Add an output clause for the specified property.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public UpdateEntityBuilder<TEntity> Output<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null,
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Output(propertyAccessor?.Column, tableAlias, columnAlias);
    }

    /// <summary>
    /// Conditionally add an output clause for the specified property.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public UpdateEntityBuilder<TEntity> OutputIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null,
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return OutputIf(propertyAccessor?.Column, tableAlias, columnAlias, condition);
    }

    /// <summary>
    /// Add a from clause to the query.
    /// </summary>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="tableSchema">The table schema.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public override UpdateEntityBuilder<TEntity> From(
        string tableName = null,
        string tableSchema = null,
        string tableAlias = null)
    {
        return base.From(
            tableName ?? _typeAccessor.TableName,
            tableSchema ?? _typeAccessor.TableSchema,
            tableAlias);
    }

    /// <summary>
    /// Add a join clause using the specified builder action
    /// </summary>
    /// <typeparam name="TRight">The right join entity</typeparam>
    /// <param name="builder">The join builder.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public UpdateEntityBuilder<TEntity> Join<TRight>(Action<JoinEntityBuilder<TEntity, TRight>> builder)
        where TRight : class
    {
        var innerBuilder = new JoinEntityBuilder<TEntity, TRight>(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return this;
    }

    /// <summary>
    /// Add a join clause using the specified builder action
    /// </summary>
    /// <typeparam name="TLeft">The left join entity</typeparam>
    /// <typeparam name="TRight">The right join entity</typeparam>
    /// <param name="builder">The join builder.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public UpdateEntityBuilder<TEntity> Join<TLeft, TRight>(Action<JoinEntityBuilder<TLeft, TRight>> builder)
        where TLeft : class
        where TRight : class
    {
        var innerBuilder = new JoinEntityBuilder<TLeft, TRight>(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return this;
    }


    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        return Where<TValue>(property, parameterValue, null, filterOperator);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Where(propertyAccessor.Column, parameterValue, tableAlias, filterOperator);
    }

    /// <summary>
    /// Create a where clause with the specified property, value, operator and table alias
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="filterOperator">The filter operator.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public UpdateEntityBuilder<TEntity> Where<TModel, TValue>(
        Expression<Func<TModel, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var typeAccessor = TypeAccessor.GetAccessor<TModel>();
        var propertyAccessor = typeAccessor.FindProperty(property);

        return Where(propertyAccessor.Column, parameterValue, tableAlias, filterOperator);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        return WhereIf(property, parameterValue, null, filterOperator, condition);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereIf(propertyAccessor.Column, parameterValue, tableAlias, filterOperator, condition);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereIn<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereIn(propertyAccessor?.Column, parameterValues, tableAlias);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, condition);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, tableAlias, condition);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereOr(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.Or);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereAnd(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.And);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }


    /// <inheritdoc />
    public override QueryStatement BuildStatement()
    {
        // add table and schema from attribute if not set
        if (TableExpression == null)
            Table(_typeAccessor.TableName, _typeAccessor.TableSchema);

        return base.BuildStatement();
    }
}
