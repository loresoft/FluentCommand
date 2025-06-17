using System.Linq.Expressions;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL WHERE clauses for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class WhereEntityBuilder<TEntity>
    : WhereBuilder<WhereEntityBuilder<TEntity>>, IWhereEntityBuilder<TEntity, WhereEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="WhereEntityBuilder{TEntity}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator used to build SQL statements.</param>
    /// <param name="parameters">The collection of query parameters.</param>
    /// <param name="logicalOperator">The logical operator to combine WHERE expressions (default is <see cref="LogicalOperators.And"/>).</param>
    public WhereEntityBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <summary>
    /// Adds a WHERE clause for the specified property, value, and filter operator.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to compare.</typeparam>
    /// <param name="property">An expression selecting the property to filter on.</param>
    /// <param name="parameterValue">The value to compare the property against.</param>
    /// <param name="filterOperator">The filter operator to use (default is <see cref="FilterOperators.Equal"/>).</param>
    /// <returns>The builder instance for chaining further calls.</returns>
    public WhereEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        return Where(property, parameterValue, null, filterOperator);
    }

    /// <summary>
    /// Adds a WHERE clause for the specified property, value, filter operator, and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to compare.</typeparam>
    /// <param name="property">An expression selecting the property to filter on.</param>
    /// <param name="parameterValue">The value to compare the property against.</param>
    /// <param name="tableAlias">The table alias to use in the query.</param>
    /// <param name="filterOperator">The filter operator to use (default is <see cref="FilterOperators.Equal"/>).</param>
    /// <returns>The builder instance for chaining further calls.</returns>
    public WhereEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Where(propertyAccessor?.Column, parameterValue, tableAlias, filterOperator);
    }

    /// <summary>
    /// Adds a WHERE IN clause for the specified property and collection of values, with an optional table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the values to compare.</typeparam>
    /// <param name="property">An expression selecting the property to filter on.</param>
    /// <param name="parameterValues">The collection of values for the IN clause.</param>
    /// <param name="tableAlias">The table alias to use in the query (optional).</param>
    /// <returns>The builder instance for chaining further calls.</returns>
    public WhereEntityBuilder<TEntity> WhereIn<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereIn(propertyAccessor?.Column, parameterValues, tableAlias);
    }

    /// <summary>
    /// Conditionally adds a WHERE IN clause for the specified property and collection of values.
    /// </summary>
    /// <typeparam name="TValue">The type of the values to compare.</typeparam>
    /// <param name="property">An expression selecting the property to filter on.</param>
    /// <param name="parameterValues">The collection of values for the IN clause.</param>
    /// <param name="condition">
    /// A function that determines whether to add the clause, based on the property name and values.
    /// If <c>null</c>, the clause is always added.
    /// </param>
    /// <returns>The builder instance for chaining further calls.</returns>
    public WhereEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, condition);
    }

    /// <summary>
    /// Conditionally adds a WHERE IN clause for the specified property, collection of values, and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the values to compare.</typeparam>
    /// <param name="property">An expression selecting the property to filter on.</param>
    /// <param name="parameterValues">The collection of values for the IN clause.</param>
    /// <param name="tableAlias">The table alias to use in the query.</param>
    /// <param name="condition">
    /// A function that determines whether to add the clause, based on the table alias and values.
    /// If <c>null</c>, the clause is always added.
    /// </param>
    /// <returns>The builder instance for chaining further calls.</returns>
    public WhereEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, tableAlias, condition);
    }

    /// <summary>
    /// Conditionally adds a WHERE clause for the specified property, value, and filter operator.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to compare.</typeparam>
    /// <param name="property">An expression selecting the property to filter on.</param>
    /// <param name="parameterValue">The value to compare the property against.</param>
    /// <param name="filterOperator">The filter operator to use (default is <see cref="FilterOperators.Equal"/>).</param>
    /// <param name="condition">
    /// A function that determines whether to add the clause, based on the property name and value.
    /// If <c>null</c>, the clause is always added.
    /// </param>
    /// <returns>The builder instance for chaining further calls.</returns>
    public WhereEntityBuilder<TEntity> WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        return WhereIf(property, parameterValue, null, filterOperator, condition);
    }

    /// <summary>
    /// Conditionally adds a WHERE clause for the specified property, value, filter operator, and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to compare.</typeparam>
    /// <param name="property">An expression selecting the property to filter on.</param>
    /// <param name="parameterValue">The value to compare the property against.</param>
    /// <param name="tableAlias">The table alias to use in the query.</param>
    /// <param name="filterOperator">The filter operator to use (default is <see cref="FilterOperators.Equal"/>).</param>
    /// <param name="condition">
    /// A function that determines whether to add the clause, based on the table alias and value.
    /// If <c>null</c>, the clause is always added.
    /// </param>
    /// <returns>The builder instance for chaining further calls.</returns>
    public WhereEntityBuilder<TEntity> WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereIf(propertyAccessor?.Column, parameterValue, tableAlias, filterOperator, condition);
    }

    /// <summary>
    /// Adds a logical OR group to the WHERE clause using the specified builder action.
    /// </summary>
    /// <param name="builder">An action that configures the logical OR group using a <see cref="LogicalEntityBuilder{TEntity}"/>.</param>
    /// <returns>The builder instance for chaining further calls.</returns>
    public WhereEntityBuilder<TEntity> WhereOr(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.Or);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }

    /// <summary>
    /// Adds a logical AND group to the WHERE clause using the specified builder action.
    /// </summary>
    /// <param name="builder">An action that configures the logical AND group using a <see cref="LogicalEntityBuilder{TEntity}"/>.</param>
    /// <returns>The builder instance for chaining further calls.</returns>
    public WhereEntityBuilder<TEntity> WhereAnd(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.And);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }

    /// <summary>
    /// Builds the SQL WHERE statement using the current expressions and parameters.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the SQL WHERE clause and its parameters,
    /// or <c>null</c> if no expressions are present.
    /// </returns>
    public override QueryStatement BuildStatement()
    {
        if (WhereExpressions == null || WhereExpressions.Count == 0)
            return null;

        var statement = QueryGenerator.BuildWhere(WhereExpressions);

        return new QueryStatement(statement, Parameters);
    }
}
