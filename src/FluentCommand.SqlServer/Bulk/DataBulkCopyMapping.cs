using System.Linq.Expressions;
using System.Reflection;

namespace FluentCommand.Bulk;

/// <summary>
/// Provides a builder for configuring column mappings for <see cref="IDataBulkCopy"/> operations using strongly typed lambda expressions.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being mapped.</typeparam>
public class DataBulkCopyMapping<TEntity>
{
    private readonly IDataBulkCopy _bulkCopy;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataBulkCopyMapping{TEntity}"/> class.
    /// </summary>
    /// <param name="bulkCopy">The <see cref="IDataBulkCopy"/> instance to configure column mappings for.</param>
    internal DataBulkCopyMapping(IDataBulkCopy bulkCopy)
    {
        _bulkCopy = bulkCopy;
    }

    /// <summary>
    /// Creates a new column mapping using a lambda expression to refer to both the source and destination columns by property name.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="sourceProperty">A lambda expression representing the property of the source column for the mapping.</param>
    /// <returns>
    /// The same <see cref="DataBulkCopyMapping{TEntity}"/> instance for fluent chaining.
    /// </returns>
    public DataBulkCopyMapping<TEntity> Mapping<TValue>(Expression<Func<TEntity, TValue>> sourceProperty)
    {
        var propertyName = ExtractColumnName(sourceProperty);
        _bulkCopy.Mapping(propertyName, propertyName);

        return this;
    }

    /// <summary>
    /// Creates a new column mapping using a lambda expression to refer to the source column and a column name for the destination column.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="sourceProperty">A lambda expression representing the property of the source column for the mapping.</param>
    /// <param name="destinationColumn">The name of the destination column within the destination table.</param>
    /// <returns>
    /// The same <see cref="DataBulkCopyMapping{TEntity}"/> instance for fluent chaining.
    /// </returns>
    public DataBulkCopyMapping<TEntity> Mapping<TValue>(Expression<Func<TEntity, TValue>> sourceProperty, string destinationColumn)
    {
        var propertyName = ExtractColumnName(sourceProperty);
        _bulkCopy.Mapping(propertyName, destinationColumn);

        return this;
    }

    /// <summary>
    /// Creates a new column mapping using a lambda expression to refer to the source column and a column ordinal for the destination column.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="sourceProperty">A lambda expression representing the property of the source column for the mapping.</param>
    /// <param name="destinationOrdinal">The ordinal position of the destination column within the destination table.</param>
    /// <returns>
    /// The same <see cref="DataBulkCopyMapping{TEntity}"/> instance for fluent chaining.
    /// </returns>
    public DataBulkCopyMapping<TEntity> Mapping<TValue>(Expression<Func<TEntity, TValue>> sourceProperty, int destinationOrdinal)
    {
        var propertyName = ExtractColumnName(sourceProperty);
        _bulkCopy.Mapping(propertyName, destinationOrdinal);

        return this;
    }

    /// <summary>
    /// Ignores the specified source column by removing it from the mapped columns collection, using a lambda expression to specify the property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="sourceProperty">A lambda expression representing the property of the source column to ignore.</param>
    /// <returns>
    /// The same <see cref="DataBulkCopyMapping{TEntity}"/> instance for fluent chaining.
    /// </returns>
    public DataBulkCopyMapping<TEntity> Ignore<TValue>(Expression<Func<TEntity, TValue>> sourceProperty)
    {
        var propertyName = ExtractColumnName(sourceProperty);
        _bulkCopy.Ignore(propertyName);

        return this;
    }

    /// <summary>
    /// Extracts the column name from the provided lambda expression, using property attributes if available.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="sourceProperty">A lambda expression representing the property.</param>
    /// <returns>The resolved column name for the property.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the expression does not represent a property access.
    /// </exception>
    private string ExtractColumnName<TValue>(Expression<Func<TEntity, TValue>> sourceProperty)
    {
        var memberExpression = sourceProperty.Body as MemberExpression;
        if (memberExpression == null)
            throw new ArgumentException("The expression is not a member access expression.", nameof(memberExpression));

        var property = memberExpression.Member as PropertyInfo;
        if (property == null)
            throw new ArgumentException("The member access expression does not access a property.", nameof(memberExpression));

        var column = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
        if (!string.IsNullOrEmpty(column?.Name))
            return column.Name;

        var display = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
        if (!string.IsNullOrEmpty(display?.Name))
            return display.Name;

        return property.Name;
    }
}
