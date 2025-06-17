using System.Linq.Expressions;

namespace FluentCommand.Query;

/// <summary>
/// Defines methods for building SQL WHERE clauses for a specific entity type with fluent, chainable methods.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TBuilder">The type of the builder returned for chaining.</typeparam>
public interface IWhereEntityBuilder<TEntity, out TBuilder>
    where TEntity : class
{
    /// <summary>
    /// Adds a WHERE clause for the specified property, value, and filter operator.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to compare.</typeparam>
    /// <param name="property">An expression selecting the property to filter on.</param>
    /// <param name="parameterValue">The value to compare the property against.</param>
    /// <param name="filterOperator">The filter operator to use (default is <see cref="FilterOperators.Equal"/>).</param>
    /// <returns>
    /// The builder instance for chaining further calls.
    /// </returns>
    TBuilder Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal);

    /// <summary>
    /// Adds a WHERE clause for the specified property, value, filter operator, and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the value to compare.</typeparam>
    /// <param name="property">An expression selecting the property to filter on.</param>
    /// <param name="parameterValue">The value to compare the property against.</param>
    /// <param name="tableAlias">The table alias to use in the query.</param>
    /// <param name="filterOperator">The filter operator to use (default is <see cref="FilterOperators.Equal"/>).</param>
    /// <returns>
    /// The builder instance for chaining further calls.
    /// </returns>
    TBuilder Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal);

    /// <summary>
    /// Adds a WHERE IN clause for the specified property and collection of values, with an optional table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the values to compare.</typeparam>
    /// <param name="property">An expression selecting the property to filter on.</param>
    /// <param name="parameterValues">The collection of values for the IN clause.</param>
    /// <param name="tableAlias">The table alias to use in the query (optional).</param>
    /// <returns>
    /// The builder instance for chaining further calls.
    /// </returns>
    TBuilder WhereIn<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias = null);

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
    /// <returns>
    /// The builder instance for chaining further calls.
    /// </returns>
    TBuilder WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        Func<string, IEnumerable<TValue>, bool> condition = null);

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
    /// <returns>
    /// The builder instance for chaining further calls.
    /// </returns>
    TBuilder WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias,
        Func<string, IEnumerable<TValue>, bool> condition = null);

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
    /// <returns>
    /// The builder instance for chaining further calls.
    /// </returns>
    TBuilder WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null);

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
    /// <returns>
    /// The builder instance for chaining further calls.
    /// </returns>
    TBuilder WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null);

    /// <summary>
    /// Adds a logical AND group to the WHERE clause using the specified builder action.
    /// </summary>
    /// <param name="builder">An action that configures the logical AND group using a <see cref="LogicalEntityBuilder{TEntity}"/>.</param>
    /// <returns>
    /// The builder instance for chaining further calls.
    /// </returns>
    TBuilder WhereAnd(Action<LogicalEntityBuilder<TEntity>> builder);

    /// <summary>
    /// Adds a logical OR group to the WHERE clause using the specified builder action.
    /// </summary>
    /// <param name="builder">An action that configures the logical OR group using a <see cref="LogicalEntityBuilder{TEntity}"/>.</param>
    /// <returns>
    /// The builder instance for chaining further calls.
    /// </returns>
    TBuilder WhereOr(Action<LogicalEntityBuilder<TEntity>> builder);
}
