using System.Linq.Expressions;
using System.Reflection;

using Microsoft.Data.SqlClient;

namespace FluentCommand.Bulk;

/// <summary>
/// A builder class for <see cref="IDataBulkCopy"/> column mapping.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class DataBulkCopyMapping<TEntity>
{
    private readonly IDataBulkCopy _bulkCopy;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataBulkCopyMapping{TEntity}"/> class.
    /// </summary>
    /// <param name="bulkCopy">The bulk copy.</param>
    internal DataBulkCopyMapping(IDataBulkCopy bulkCopy)
    {
        _bulkCopy = bulkCopy;
    }

    /// <summary>
    /// Creates a new column mapping using a Lamba express to refer to source and destination columns.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="sourceProperty">A Lambda expression representing the property of the source column for the mapping.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public DataBulkCopyMapping<TEntity> Mapping<TValue>(Expression<Func<TEntity, TValue>> sourceProperty)
    {
        var propertyName = ExtractColumnName(sourceProperty);
        _bulkCopy.Mapping(propertyName, propertyName);

        return this;
    }

    /// <summary>
    /// Creates a new column mapping, using a Lamba express to refer to source column and a column name for the target column.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="sourceProperty">A Lambda expression representing the property of the source column for the mapping.</param>
    /// <param name="destinationColumn">The name of the destination column within the destination table.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public DataBulkCopyMapping<TEntity> Mapping<TValue>(Expression<Func<TEntity, TValue>> sourceProperty, string destinationColumn)
    {
        var propertyName = ExtractColumnName(sourceProperty);
        _bulkCopy.Mapping(propertyName, destinationColumn);

        return this;
    }

    /// <summary>
    /// Creates a new column mapping, using a Lamba express to refer to source column and a column ordinal for the target column.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="sourceProperty">A Lambda expression representing the property of the source column for the mapping.</param>
    /// <param name="destinationOrdinal">The ordinal position of the destination column within the destination table.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public DataBulkCopyMapping<TEntity> Mapping<TValue>(Expression<Func<TEntity, TValue>> sourceProperty, int destinationOrdinal)
    {
        var propertyName = ExtractColumnName(sourceProperty);
        _bulkCopy.Mapping(propertyName, destinationOrdinal);

        return this;
    }

    /// <summary>
    /// Ignores the specified source column by removing it from the mapped columns collection.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="sourceProperty">A Lambda expression representing the property of the source column for the mapping.</param>
    /// <returns>
    /// A fluent <see langword="interface" /> to a <see cref="SqlBulkCopy " /> operation.
    /// </returns>
    public DataBulkCopyMapping<TEntity> Ignore<TValue>(Expression<Func<TEntity, TValue>> sourceProperty)
    {
        var propertyName = ExtractColumnName(sourceProperty);
        _bulkCopy.Ignore(propertyName);

        return this;
    }


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
