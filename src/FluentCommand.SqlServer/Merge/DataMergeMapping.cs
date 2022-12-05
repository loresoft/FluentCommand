using System.Linq.Expressions;
using System.Reflection;

using FluentCommand.Extensions;

namespace FluentCommand.Merge;

/// <summary>
/// Fluent class for building strongly typed data merge mapping
/// </summary>
/// <typeparam name="TEntity">The type of the entity used in the mapping.</typeparam>
public class DataMergeMapping<TEntity> : DataMergeMapping, IDataMergeMapping<TEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMergeMapping{TEntity}"/> class.
    /// </summary>
    /// <param name="mergeDefinition">The data merge definition.</param>
    public DataMergeMapping(DataMergeDefinition mergeDefinition)
        : base(mergeDefinition)
    {
    }

    /// <summary>
    /// Automatically maps all properties in <typeparamref name="TEntity"/> as columns.
    /// </summary>
    /// <returns></returns>
    public IDataMergeMapping<TEntity> AutoMap()
    {
        DataMergeDefinition.AutoMap<TEntity>(MergeDefinition);

        return this;
    }

    /// <summary>
    /// Start column mapping for the specified source column name.
    /// </summary>
    /// <typeparam name="TValue">The property value type.</typeparam>
    /// <param name="sourceProperty">The source property.</param>
    /// <returns>
    /// a fluent <c>interface</c> for mapping a data merge column.
    /// </returns>
    public IDataColumnMapping Column<TValue>(Expression<Func<TEntity, TValue>> sourceProperty)
    {
        if (sourceProperty == null)
            throw new ArgumentNullException(nameof(sourceProperty));

        string sourceColumn = ExtractName(sourceProperty);
        return Column(sourceColumn);
    }

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
/// Fluent class for building data merge mapping
/// </summary>
public class DataMergeMapping : IDataMergeMapping
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMergeMapping"/> class.
    /// </summary>
    /// <param name="mergeDefinition">The data merge definition.</param>
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
    /// Start column mapping for the specified source column name.
    /// </summary>
    /// <param name="sourceColumn">The source column name.</param>
    /// <returns>a fluent <c>interface</c> for mapping a data merge column.</returns>
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
