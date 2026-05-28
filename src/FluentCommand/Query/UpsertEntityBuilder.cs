using System.Linq.Expressions;
using System.ComponentModel.DataAnnotations;

using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL UPSERT statements for a specific entity type with fluent, chainable methods.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class UpsertEntityBuilder<TEntity> : UpsertBuilder<UpsertEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpsertEntityBuilder{TEntity}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    public UpsertEntityBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters)
        : base(queryGenerator, parameters)
    {
    }

    /// <summary>
    /// Adds a value for the specified entity property and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">An expression selecting the property to insert or update.</param>
    /// <param name="parameterValue">The value to insert or update for the property.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public UpsertEntityBuilder<TEntity> Value<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue? parameterValue)
    {
        var propertyAccessor = GetPropertyAccessor(property);
        return Value(propertyAccessor.Column, parameterValue);
    }

    /// <summary>
    /// Conditionally adds a value for the specified entity property and value if the condition is met.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">An expression selecting the property to insert or update.</param>
    /// <param name="parameterValue">The value to insert or update for the property.</param>
    /// <param name="condition">A function that determines whether to add the value, based on the property name and value.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public UpsertEntityBuilder<TEntity> ValueIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue? parameterValue,
        Func<string, TValue?, bool> condition)
    {
        var propertyAccessor = GetPropertyAccessor(property);
        return ValueIf(propertyAccessor.Column, parameterValue, condition);
    }

    /// <summary>
    /// Adds values from the specified entity. If column names are provided,
    /// only those that match an entity property name will be included. Key columns are inferred from properties marked with <see cref="KeyAttribute"/>.
    /// </summary>
    /// <param name="entity">The entity to insert or update.</param>
    /// <param name="columnNames">The column names to include (optional).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <c>null</c>.</exception>
    public UpsertEntityBuilder<TEntity> Values(
        TEntity entity,
        IEnumerable<string>? columnNames = null)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var properties = _typeAccessor.GetProperties();
        if (properties == null)
            return this;

        var columnSet = new HashSet<string>(columnNames ?? [], StringComparer.OrdinalIgnoreCase);

        foreach (var property in properties)
        {
            if (columnSet.Count > 0 && !columnSet.Contains(property.Column))
                continue;

            if (property.IsNotMapped || property.IsDatabaseGenerated)
                continue;

            Value(property.Column, property.GetValue(entity), property.MemberType);
        }

        AddEntityKeys();

        return this;
    }

    /// <summary>
    /// Adds a key column used to determine whether a row already exists.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the key property.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public UpsertEntityBuilder<TEntity> Key<TValue>(Expression<Func<TEntity, TValue>> property)
    {
        var propertyAccessor = GetPropertyAccessor(property);
        return Key(propertyAccessor.Column);
    }

    /// <summary>
    /// Conditionally adds a key column used to determine whether a row already exists.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the key property.</param>
    /// <param name="condition">A function that determines whether to add the key column.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public UpsertEntityBuilder<TEntity> KeyIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        Func<string, bool>? condition = null)
    {
        var propertyAccessor = GetPropertyAccessor(property);
        return KeyIf(propertyAccessor.Column, condition);
    }

    /// <summary>
    /// Adds an OUTPUT clause for the specified entity property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to output.</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <param name="columnAlias">The alias for the output column (optional).</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public UpsertEntityBuilder<TEntity> Output<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string? tableAlias = null,
        string? columnAlias = null)
    {
        var propertyAccessor = GetPropertyAccessor(property);
        return Output(propertyAccessor.Column, tableAlias, columnAlias);
    }

    /// <summary>
    /// Conditionally adds an OUTPUT clause for the specified entity property if the condition is met.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to output.</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <param name="columnAlias">The alias for the output column (optional).</param>
    /// <param name="condition">A function that determines whether to add the OUTPUT clause.</param>
    /// <returns>The same builder instance for method chaining.</returns>
    public UpsertEntityBuilder<TEntity> OutputIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string? tableAlias = null,
        string? columnAlias = null,
        Func<string, bool>? condition = null)
    {
        var propertyAccessor = GetPropertyAccessor(property);
        return OutputIf(propertyAccessor.Column, tableAlias, columnAlias, condition);
    }

    /// <summary>
    /// Builds the SQL UPSERT statement using the current configuration.
    /// </summary>
    /// <returns>A <see cref="QueryStatement"/> containing the SQL UPSERT statement and its parameters.</returns>
    public override QueryStatement? BuildStatement()
    {
        if (TableExpression == null)
            Into(_typeAccessor.TableName, _typeAccessor.TableSchema);

        AddEntityKeys();

        return base.BuildStatement();
    }

    private void AddEntityKeys()
    {
        var properties = _typeAccessor.GetProperties();
        foreach (var property in properties)
        {
            if (property.IsNotMapped || !property.IsKey)
                continue;

            Key(property.Column);
        }
    }

    private static IMemberAccessor GetPropertyAccessor<TModel, TValue>(
        TypeAccessor typeAccessor,
        Expression<Func<TModel, TValue>> property)
    {
        ArgumentNullException.ThrowIfNull(property);

        var propertyAccessor = typeAccessor.FindProperty(property);
        if (propertyAccessor is null)
            throw new ArgumentException("The specified property does not exist on the entity.", nameof(property));

        return propertyAccessor;
    }

    private static IMemberAccessor GetPropertyAccessor<TValue>(Expression<Func<TEntity, TValue>> property)
        => GetPropertyAccessor(_typeAccessor, property);
}
