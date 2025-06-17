using System.Linq.Expressions;

using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL ORDER BY clauses for a specific entity type with fluent, chainable methods.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class OrderEntityBuilder<TEntity> : OrderBuilder<OrderEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderEntityBuilder{TEntity}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    public OrderEntityBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Adds an ORDER BY clause with the specified entity property and sort direction.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to sort by.</param>
    /// <param name="sortDirection">The sort direction (default is <see cref="SortDirections.Ascending"/>).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public OrderEntityBuilder<TEntity> OrderBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderBy(propertyAccessor.Column, null, sortDirection);
    }

    /// <summary>
    /// Adds an ORDER BY clause with the specified entity property, sort direction, and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to sort by.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="sortDirection">The sort direction (default is <see cref="SortDirections.Ascending"/>).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public OrderEntityBuilder<TEntity> OrderBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderBy(propertyAccessor.Column, tableAlias, sortDirection);
    }

    /// <summary>
    /// Conditionally adds an ORDER BY clause with the specified entity property, sort direction, and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to sort by.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="sortDirection">The sort direction (default is <see cref="SortDirections.Ascending"/>).</param>
    /// <param name="condition">A function that determines whether to add the ORDER BY clause, based on the property name. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public OrderEntityBuilder<TEntity> OrderByIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null,
        SortDirections sortDirection = SortDirections.Ascending,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderByIf(propertyAccessor.Column, tableAlias, sortDirection, condition);
    }
}
