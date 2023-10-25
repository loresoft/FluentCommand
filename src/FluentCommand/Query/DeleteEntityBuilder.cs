using System.Linq.Expressions;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;


/// <summary>
/// Delete query statement builder
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class DeleteEntityBuilder<TEntity>
    : DeleteBuilder<DeleteEntityBuilder<TEntity>>, IWhereEntityBuilder<TEntity, DeleteEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteEntityBuilder{TEntity}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The parameters.</param>
    /// <param name="logicalOperator">The logical operator.</param>
    public DeleteEntityBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
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
    public DeleteEntityBuilder<TEntity> Output<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = "DELETED",
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Output(propertyAccessor.Column, tableAlias, columnAlias);
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
    public DeleteEntityBuilder<TEntity> OutputIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = "DELETED",
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return OutputIf(propertyAccessor.Column, tableAlias, columnAlias, condition);
    }

    /// <inheritdoc />
    public override DeleteEntityBuilder<TEntity> From(
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
    public DeleteEntityBuilder<TEntity> Join<TRight>(Action<JoinEntityBuilder<TEntity, TRight>> builder)
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
    public DeleteEntityBuilder<TEntity> Join<TLeft, TRight>(Action<JoinEntityBuilder<TLeft, TRight>> builder)
        where TLeft : class
        where TRight : class
    {
        var innerBuilder = new JoinEntityBuilder<TLeft, TRight>(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return this;
    }


    /// <inheritdoc />
    public DeleteEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        return Where<TValue>(property, parameterValue, null, filterOperator);
    }

    /// <inheritdoc />
    public DeleteEntityBuilder<TEntity> Where<TValue>(
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
    public DeleteEntityBuilder<TEntity> Where<TModel, TValue>(
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
    public DeleteEntityBuilder<TEntity> WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        return WhereIf(property, parameterValue, null, filterOperator, condition);
    }

    /// <inheritdoc />
    public DeleteEntityBuilder<TEntity> WhereIf<TValue>(
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
    public DeleteEntityBuilder<TEntity> WhereIn<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereIn(propertyAccessor?.Column, parameterValues, tableAlias);
    }

    /// <inheritdoc />
    public DeleteEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, condition);
    }

    /// <inheritdoc />
    public DeleteEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, tableAlias, condition);
    }

    /// <inheritdoc />
    public DeleteEntityBuilder<TEntity> WhereOr(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.Or);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }

    /// <inheritdoc />
    public DeleteEntityBuilder<TEntity> WhereAnd(Action<LogicalEntityBuilder<TEntity>> builder)
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
