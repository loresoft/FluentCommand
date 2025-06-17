using System.Linq.Expressions;

namespace FluentCommand.Query;

/// <summary>
/// interface for where clause builder
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
public interface IWhereEntityBuilder<TEntity, out TBuilder>
    where TEntity : class
{
    /// <summary>
    /// Create a where clause with the specified property, value and operator
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="filterOperator">The filter operator.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    TBuilder Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal);

    /// <summary>
    /// Create a where clause with the specified property, value, operator and table alias
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="filterOperator">The filter operator.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    TBuilder Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal);

    /// <summary>
    /// Create a where in clause with the specified property, values and table alias
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValues">The parameter values.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    TBuilder WhereIn<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias = null);

    /// <summary>
    /// Conditionally create a where in clause with the specified property and values
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValues">The parameter values.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    TBuilder WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        Func<string, IEnumerable<TValue>, bool> condition = null);

    /// <summary>
    /// Conditionally create a where in clause with the specified property, values and table alias
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValues">The parameter values.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    TBuilder WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias,
        Func<string, IEnumerable<TValue>, bool> condition = null);

    /// <summary>
    /// Conditionally create a where clause with the specified property, value and operator
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="filterOperator">The filter operator.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    TBuilder WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null);

    /// <summary>
    /// Conditionally create a where clause with the specified property, value, operator and table alias
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="filterOperator">The filter operator.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    TBuilder WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null);

    /// <summary>
    /// Create a logical AND where clause group
    /// </summary>
    /// <param name="builder">The logical AND where clause builder.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    TBuilder WhereAnd(Action<LogicalEntityBuilder<TEntity>> builder);

    /// <summary>
    /// Create a logical OR where clause group
    /// </summary>
    /// <param name="builder">The logical OR where clause builder.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    TBuilder WhereOr(Action<LogicalEntityBuilder<TEntity>> builder);
}
