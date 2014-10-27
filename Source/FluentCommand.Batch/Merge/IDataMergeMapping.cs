using System;
using System.Linq.Expressions;

namespace FluentCommand.Merge
{
    /// <summary>
    /// A fluent <see langword="interface" /> for a data merge mapping.
    /// </summary>
    public interface IDataMergeMapping
    {
        /// <summary>
        /// Start column mapping for the specified source column name.
        /// </summary>
        /// <param name="sourceColumn">The source column name.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> for a data merge mapping.
        /// </returns>
        IDataColumnMapping Column(string sourceColumn);
    }

    /// <summary>
    /// A fluent <see langword="interface" /> for a strongly typed data merge mapping.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity being mapped</typeparam>
    public interface IDataMergeMapping<TEntity> : IDataMergeMapping
    {
        /// <summary>
        /// Automatically maps all properties in <typeparamref name="TEntity"/> as columns.
        /// </summary>
        /// <returns>
        /// </returns>
        IDataMergeMapping<TEntity> AutoMap();

        /// <summary>
        /// Start column mapping for the specified source column name.
        /// </summary>
        /// <typeparam name="TValue">The property value type.</typeparam>
        /// <param name="sourceProperty">The source property.</param>
        /// <returns>
        /// A fluent <see langword="interface" /> for a data merge mapping.
        /// </returns>
        IDataColumnMapping Column<TValue>(Expression<Func<TEntity, TValue>> sourceProperty);
    }
}