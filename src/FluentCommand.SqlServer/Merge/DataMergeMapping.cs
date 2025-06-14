using System.Linq.Expressions;
using System.Reflection;

using FluentCommand.Extensions;

namespace FluentCommand.Merge;

/// <summary>
/// Provides a fluent class for building strongly typed data merge mappings for a specific entity type.
/// </summary>
/// <typeparam name="TEntity">The type of the entity used in the mapping.</typeparam>
public class DataMergeMapping<TEntity> : DataMergeMapping, IDataMergeMapping<TEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMergeMapping{TEntity}"/> class.
    /// </summary>
    /// <param name="mergeDefinition">The <see cref="DataMergeDefinition"/> that defines the merge operation.</param>
    public DataMergeMapping(DataMergeDefinition mergeDefinition)
        : base(mergeDefinition)
    {
    }

    /// <summary>
    /// Automatically maps all public properties of <typeparamref name="TEntity"/> as columns in the merge definition.
    /// </summary>
    /// <returns>
    /// The current <see cref="IDataMergeMapping{TEntity}"/> instance for chaining.
    /// </returns>
    public IDataMergeMapping<TEntity> AutoMap()
    {
        DataMergeDefinition.AutoMap<TEntity>(MergeDefinition);

        return this;
    }

    /// <summary>
    /// Begins configuration of a column mapping for the specified entity property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="sourceProperty">An expression selecting the source property to map.</param>
    /// <returns>
    /// An <see cref="IDataColumnMapping"/> interface for further column mapping configuration.
    /// </returns>
    public IDataColumnMapping Column<TValue>(Expression<Func<TEntity, TValue>> sourceProperty)
    {
        if (sourceProperty == null)
            throw new ArgumentNullException(nameof(sourceProperty));

        string sourceColumn = ExtractName(sourceProperty);
        return Column(sourceColumn);
    }

    /// <summary>
    /// Extracts the column name from the given property expression, using attributes if available.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="propertyExpression">The property expression.</param>
    /// <returns>The resolved column name.</returns>
    /// <exception cref="ArgumentException">Thrown if the expression does not represent a property access.</exception>
    private string ExtractName<TValue>(Expression<Func<TEntity, TValue>> propertyExpression)
    {
        var memberExpression = propertyExpression.Body as MemberExpression;
        if (memberExpression == null)
            throw new ArgumentException("The expression is not a member access expression.", nameof(propertyExpression));

        var property = memberExpression.Member as PropertyInfo;
        if (property == null)
            throw new ArgumentException("The member access expression does not access a property.", nameof(propertyExpression));

        var column = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>();
        if (!string.IsNullOrEmpty(column?.Name))
            return column.Name;

        var display = property.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
        if (!string.IsNullOrEmpty(display?.Name))
            return display.Name;

        return property.Name;
    }
}

/// <summary>
/// Provides a fluent class for building data merge mappings.
/// </summary>
public class DataMergeMapping : IDataMergeMapping
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMergeMapping"/> class.
    /// </summary>
    /// <param name="mergeDefinition">The <see cref="DataMergeDefinition"/> that defines the merge operation.</param>
    public DataMergeMapping(DataMergeDefinition mergeDefinition)
    {
        MergeDefinition = mergeDefinition;
    }

    /// <summary>
    /// Gets the current <see cref="DataMergeDefinition"/> being updated.
    /// </summary>
    /// <value>
    /// The current data merge definition.
    /// </value>
    public DataMergeDefinition MergeDefinition { get; }

    /// <summary>
    /// Begins configuration of a column mapping for the specified source column name.
    /// </summary>
    /// <param name="sourceColumn">The name of the source column to map.</param>
    /// <returns>
    /// An <see cref="IDataColumnMapping"/> interface for further column mapping configuration.
    /// </returns>
    public IDataColumnMapping Column(string sourceColumn)
    {
        var mergeColumn = MergeDefinition.Columns.FirstOrAdd(
            c => c.SourceColumn == sourceColumn,
            () => new DataMergeColumn
            {
                SourceColumn = sourceColumn,
                TargetColumn = sourceColumn
            });

        var columnMapping = new DataMergeColumnMapping(mergeColumn);

        return columnMapping;
    }
}
