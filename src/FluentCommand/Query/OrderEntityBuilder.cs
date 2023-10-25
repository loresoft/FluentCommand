using System.Linq.Expressions;

using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Query order by builder
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class OrderEntityBuilder<TEntity> : OrderBuilder<OrderEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderEntityBuilder{TEntity}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    public OrderEntityBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Add an order by clause with the specified property and sort direction.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public OrderEntityBuilder<TEntity> OrderBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderBy(propertyAccessor.Column, null, sortDirection);
    }

    /// <summary>
    /// Add an order by clause with the specified property, sort direction and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Conditionally add an order by clause with the specified property, sort direction and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
