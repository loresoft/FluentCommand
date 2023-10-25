using System.Linq.Expressions;

using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Insert query statement builder
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class InsertEntityBuilder<TEntity> : InsertBuilder<InsertEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="InsertEntityBuilder{TEntity}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    public InsertEntityBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Adds a value with specified property and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public InsertEntityBuilder<TEntity> Value<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Value(propertyAccessor.Column, parameterValue);
    }

    /// <summary>
    /// Conditionally adds a value with specified property and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public InsertEntityBuilder<TEntity> ValueIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        Func<string, TValue, bool> condition)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return ValueIf(propertyAccessor.Column, parameterValue, condition);
    }

    /// <summary>
    /// Adds a values from the specified entity. If column names are passed in,
    /// only those that match an entity property name will be included.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="columnNames">The column names to include.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public InsertEntityBuilder<TEntity> Values(
        TEntity entity,
        IEnumerable<string> columnNames = null)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var properties = _typeAccessor.GetProperties();
        var columnSet = new HashSet<string>(columnNames ?? Enumerable.Empty<string>());

        foreach (var property in properties)
        {
            if (columnSet.Count > 0 && !columnSet.Contains(property.Column))
                continue;

            if (property.IsNotMapped || property.IsDatabaseGenerated)
                continue;

            // include the type to prevent issues with null
            Value(property.Column, property.GetValue(entity), property.MemberType);
        }

        return this;
    }

    /// <summary>
    /// Add an output clause for the specified property.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public InsertEntityBuilder<TEntity> Output<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = "INSERTED",
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Output(propertyAccessor.Column, tableAlias, columnAlias);
    }

    /// <summary>
    /// Conditionally add an output clause for the specified property.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public InsertEntityBuilder<TEntity> OutputIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = "INSERTED",
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return OutputIf(propertyAccessor.Column, tableAlias, columnAlias, condition);
    }

    /// <inheritdoc />
    public override QueryStatement BuildStatement()
    {
        // add table and schema from attribute if not set
        if (TableExpression == null)
            Into(_typeAccessor.TableName, _typeAccessor.TableSchema);

        return base.BuildStatement();
    }
}
