using System.Linq.Expressions;

namespace FluentCommand.Merge;

/// <summary>
/// Provides a fluent interface for configuring data merge column mappings.
/// </summary>
public interface IDataMergeMapping
{
    /// <summary>
    /// Begins configuration of a column mapping for the specified source column name.
    /// </summary>
    /// <param name="sourceColumn">The name of the source column to map.</param>
    /// <returns>
    /// An <see cref="IDataColumnMapping"/> interface for further column mapping configuration.
    /// </returns>
    IDataColumnMapping Column(string sourceColumn);
}

/// <summary>
/// Provides a strongly typed, fluent interface for configuring data merge column mappings for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being mapped.</typeparam>
public interface IDataMergeMapping<TEntity> : IDataMergeMapping
{
    /// <summary>
    /// Automatically maps all public properties of <typeparamref name="TEntity"/> as columns.
    /// </summary>
    /// <returns>
    /// The current <see cref="IDataMergeMapping{TEntity}"/> instance for chaining.
    /// </returns>
    IDataMergeMapping<TEntity> AutoMap();

    /// <summary>
    /// Begins configuration of a column mapping for the specified entity property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="sourceProperty">An expression selecting the source property to map.</param>
    /// <returns>
    /// An <see cref="IDataColumnMapping"/> interface for further column mapping configuration.
    /// </returns>
    IDataColumnMapping Column<TValue>(Expression<Func<TEntity, TValue>> sourceProperty);
}
