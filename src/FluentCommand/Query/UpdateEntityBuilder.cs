using System.Linq.Expressions;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL UPDATE statements for a specific entity type with fluent, chainable methods.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class UpdateEntityBuilder<TEntity>
    : UpdateBuilder<UpdateEntityBuilder<TEntity>>, IWhereEntityBuilder<TEntity, UpdateEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateEntityBuilder{TEntity}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    public UpdateEntityBuilder(
        IQueryGenerator queryGenerator,
        List<QueryParameter> parameters,
        LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <summary>
    /// Adds a value for the specified entity property and value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">An expression selecting the property to update.</param>
    /// <param name="parameterValue">The value to set for the property.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public UpdateEntityBuilder<TEntity> Value<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Value(propertyAccessor?.Column, parameterValue);
    }

    /// <summary>
    /// Conditionally adds a value for the specified entity property and value if the condition is met.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">An expression selecting the property to update.</param>
    /// <param name="parameterValue">The value to set for the property.</param>
    /// <param name="condition">A function that determines whether to add the value, based on the property name and value. If <c>null</c>, the value is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public UpdateEntityBuilder<TEntity> ValueIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        Func<string, TValue, bool> condition)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return ValueIf(propertyAccessor?.Column, parameterValue, condition);
    }

    /// <summary>
    /// Adds values from the specified entity. If column names are provided,
    /// only those that match an entity property name will be included.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <param name="columnNames">The column names to include (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entity"/> is <c>null</c>.</exception>
    public UpdateEntityBuilder<TEntity> Values(
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
    public UpdateEntityBuilder<TEntity> Output<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null,
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return Output(propertyAccessor?.Column, tableAlias, columnAlias);
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
    public UpdateEntityBuilder<TEntity> OutputIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null,
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);
        return OutputIf(propertyAccessor?.Column, tableAlias, columnAlias, condition);
    }

    /// <summary>
    /// Sets the target table for the UPDATE statement using the entity's mapping information by default.
    /// </summary>
    /// <param name="tableName">The name of the table (optional, defaults to entity mapping).</param>
    /// <param name="tableSchema">The schema of the table (optional, defaults to entity mapping).</param>
    /// <param name="tableAlias">The alias for the table (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public override UpdateEntityBuilder<TEntity> From(
        string tableName = null,
        string tableSchema = null,
        string tableAlias = null)
    {
        return base.From(
            tableName ?? _typeAccessor.TableName,
            tableSchema ?? _typeAccessor.TableSchema,
            tableAlias);
    }

    /// <summary>
    /// Adds a JOIN clause to the UPDATE statement using the specified builder action for the right entity.
    /// </summary>
    /// <typeparam name="TRight">The type of the right join entity.</typeparam>
    /// <param name="builder">An action that configures the join using a <see cref="JoinEntityBuilder{TEntity, TRight}"/>.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public UpdateEntityBuilder<TEntity> Join<TRight>(Action<JoinEntityBuilder<TEntity, TRight>> builder)
        where TRight : class
    {
        var innerBuilder = new JoinEntityBuilder<TEntity, TRight>(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return this;
    }

    /// <summary>
    /// Adds a JOIN clause to the UPDATE statement using the specified builder action for the left and right entities.
    /// </summary>
    /// <typeparam name="TLeft">The type of the left join entity.</typeparam>
    /// <typeparam name="TRight">The type of the right join entity.</typeparam>
    /// <param name="builder">An action that configures the join using a <see cref="JoinEntityBuilder{TLeft, TRight}"/>.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public UpdateEntityBuilder<TEntity> Join<TLeft, TRight>(Action<JoinEntityBuilder<TLeft, TRight>> builder)
        where TLeft : class
        where TRight : class
    {
        var innerBuilder = new JoinEntityBuilder<TLeft, TRight>(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return this;
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        return Where<TValue>(property, parameterValue, null, filterOperator);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Where(propertyAccessor.Column, parameterValue, tableAlias, filterOperator);
    }

    /// <summary>
    /// Adds a WHERE clause for the specified model property, value, filter operator, and table alias.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <typeparam name="TValue">The type of the value to compare.</typeparam>
    /// <param name="property">An expression selecting the property to filter on.</param>
    /// <param name="parameterValue">The value to compare the property against.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="filterOperator">The filter operator to use (default is <see cref="FilterOperators.Equal"/>).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public UpdateEntityBuilder<TEntity> Where<TModel, TValue>(
        Expression<Func<TModel, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var typeAccessor = TypeAccessor.GetAccessor<TModel>();
        var propertyAccessor = typeAccessor.FindProperty(property);

        return Where(propertyAccessor.Column, parameterValue, tableAlias, filterOperator);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        return WhereIf(property, parameterValue, null, filterOperator, condition);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereIf(propertyAccessor.Column, parameterValue, tableAlias, filterOperator, condition);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereIn<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereIn(propertyAccessor?.Column, parameterValues, tableAlias);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, condition);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, tableAlias, condition);
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereOr(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.Or);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }

    /// <inheritdoc />
    public UpdateEntityBuilder<TEntity> WhereAnd(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.And);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }

    /// <summary>
    /// Builds the SQL UPDATE statement using the current configuration.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the SQL UPDATE statement and its parameters.
    /// </returns>
    public override QueryStatement BuildStatement()
    {
        // add table and schema from attribute if not set
        if (TableExpression == null)
            Table(_typeAccessor.TableName, _typeAccessor.TableSchema);

        return base.BuildStatement();
    }
}
