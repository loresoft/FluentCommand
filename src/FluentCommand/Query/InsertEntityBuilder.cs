using System.Linq.Expressions;

using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL INSERT statements for a specific entity type with fluent, chainable methods.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class InsertEntityBuilder<TEntity> : InsertBuilder<InsertEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="InsertEntityBuilder{TEntity}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    public InsertEntityBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Adds a value for the specified entity property and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">An expression selecting the property to insert.</param>
    /// <param name="parameterValue">The value to insert for the property.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public InsertEntityBuilder<TEntity> Value<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Value(propertyAccessor.Column, parameterValue);
    }

    /// <summary>
    /// Conditionally adds a value for the specified entity property and value if the condition is met.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">An expression selecting the property to insert.</param>
    /// <param name="parameterValue">The value to insert for the property.</param>
    /// <param name="condition">A function that determines whether to add the value, based on the property name and value. If <c>null</c>, the value is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
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
    /// Adds values from the specified entity. If column names are provided,
    /// only those that match an entity property name will be included.
    /// </summary>
    /// <param name="entity">The entity to insert.</param>
    /// <param name="columnNames">The column names to include (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <c>null</c>.</exception>
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
    /// Adds an OUTPUT clause for the specified entity property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to output.</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <param name="columnAlias">The alias for the output column (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public InsertEntityBuilder<TEntity> Output<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null,
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Output(propertyAccessor.Column, tableAlias, columnAlias);
    }

    /// <summary>
    /// Conditionally adds an OUTPUT clause for the specified entity property if the condition is met.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to output.</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <param name="columnAlias">The alias for the output column (optional).</param>
    /// <param name="condition">A function that determines whether to add the OUTPUT clause. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public InsertEntityBuilder<TEntity> OutputIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null,
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return OutputIf(propertyAccessor.Column, tableAlias, columnAlias, condition);
    }

    /// <summary>
    /// Builds the SQL INSERT statement using the current configuration.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the SQL INSERT statement and its parameters.
    /// </returns>
    public override QueryStatement BuildStatement()
    {
        // add table and schema from attribute if not set
        if (TableExpression == null)
            Into(_typeAccessor.TableName, _typeAccessor.TableSchema);

        return base.BuildStatement();
    }
}
