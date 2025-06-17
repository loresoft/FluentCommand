using System.Linq.Expressions;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Provides a builder for constructing SQL SELECT statements for a specific entity type with fluent, chainable methods.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public class SelectEntityBuilder<TEntity>
    : SelectBuilder<SelectEntityBuilder<TEntity>>, IWhereEntityBuilder<TEntity, SelectEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectEntityBuilder{TEntity}"/> class.
    /// </summary>
    /// <param name="queryGenerator">The <see cref="IQueryGenerator"/> used to generate SQL expressions.</param>
    /// <param name="parameters">The list of <see cref="QueryParameter"/> objects for the query.</param>
    /// <param name="logicalOperator">The logical operator (<see cref="LogicalOperators"/>) to combine WHERE expressions. Defaults to <see cref="LogicalOperators.And"/>.</param>
    public SelectEntityBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <summary>
    /// Adds a column expression for the specified entity property.
    /// </summary>
    /// <param name="property">An expression selecting the property to include in the SELECT clause.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="columnAlias">The alias for the column (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> Column(
        Expression<Func<TEntity, object>> property,
        string tableAlias = null,
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        // alias column as property name if they don't match
        if (propertyAccessor.Name == propertyAccessor.Column)
            return Column(propertyAccessor.Column, tableAlias, columnAlias);
        else
            return Column(propertyAccessor.Column, tableAlias, columnAlias ?? propertyAccessor.Name);
    }

    /// <summary>
    /// Adds a column expression for the specified property of a model type.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="property">An expression selecting the property to include in the SELECT clause.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="columnAlias">The alias for the column (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> Column<TModel>(
        Expression<Func<TModel, object>> property,
        string tableAlias = null,
        string columnAlias = null) where TModel : class
    {
        var typeAccessor = TypeAccessor.GetAccessor<TModel>();
        var propertyAccessor = typeAccessor.FindProperty(property);

        // alias column as property name if they don't match
        if (propertyAccessor.Name == propertyAccessor.Column)
            return Column(propertyAccessor.Column, tableAlias, columnAlias);
        else
            return Column(propertyAccessor.Column, tableAlias, columnAlias ?? propertyAccessor.Name);
    }

    /// <summary>
    /// Conditionally adds a column expression for the specified entity property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to include in the SELECT clause.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="columnAlias">The alias for the column (optional).</param>
    /// <param name="condition">A function that determines whether to add the column, based on the column name. If <c>null</c>, the column is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> ColumnIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null,
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        // alias column as property name if they don't match
        if (propertyAccessor.Name == propertyAccessor.Column)
            return ColumnIf(propertyAccessor.Column, tableAlias, columnAlias, condition);
        else
            return ColumnIf(propertyAccessor.Column, tableAlias, columnAlias ?? propertyAccessor.Name, condition);
    }

    /// <summary>
    /// Adds a column expression for each of the specified column names, using entity property mapping.
    /// </summary>
    /// <param name="columnNames">The collection of column names to include in the SELECT clause.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="columnNames"/> is <c>null</c>.</exception>
    public override SelectEntityBuilder<TEntity> Columns(
        IEnumerable<string> columnNames,
        string tableAlias = null)
    {
        if (columnNames is null)
            throw new ArgumentNullException(nameof(columnNames));

        foreach (var column in columnNames)
        {
            var propertyAccessor = _typeAccessor.FindColumn(column);
            if (propertyAccessor is null)
                continue;

            // alias column as property name if they don't match
            if (propertyAccessor.Name == propertyAccessor.Column)
                Column(propertyAccessor.Column, tableAlias);
            else
                Column(propertyAccessor.Column, tableAlias, propertyAccessor.Name);
        }

        return this;
    }

    /// <summary>
    /// Adds a column expression for each property in <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="filter">An optional filter to include properties.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> Columns(
        string tableAlias = null,
        Func<IMemberAccessor, bool> filter = null)
    {
        var properties = _typeAccessor.GetProperties();

        foreach (var property in properties)
        {
            if (property.IsNotMapped)
                continue;

            if (filter != null && !filter(property))
                continue;

            // alias column as property name if they don't match
            if (property.Name == property.Column)
                Column(property.Column, tableAlias);
            else
                Column(property.Column, tableAlias, property.Name);
        }

        return this;
    }

    /// <summary>
    /// Adds a column expression for each property in <typeparamref name="TModel"/>.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="filter">An optional filter to include properties.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> Columns<TModel>(
        string tableAlias = null,
        Func<IMemberAccessor, bool> filter = null)
    {
        var typeAccessor = TypeAccessor.GetAccessor(typeof(TModel));
        var properties = typeAccessor.GetProperties();

        foreach (var property in properties)
        {
            if (property.IsNotMapped)
                continue;

            if (filter != null && !filter(property))
                continue;

            // alias column as property name if they don't match
            if (property.Name == property.Column)
                Column(property.Column, tableAlias);
            else
                Column(property.Column, tableAlias, property.Name);
        }

        return this;
    }

    /// <summary>
    /// Adds a COUNT aggregate expression using the specified entity property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to count.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="columnAlias">The alias for the column (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> Count<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null,
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Count(propertyAccessor.Column, tableAlias, columnAlias);
    }

    /// <summary>
    /// Adds an aggregate expression using the specified function and entity property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to aggregate.</param>
    /// <param name="function">The aggregate function to use (e.g., <see cref="AggregateFunctions.Sum"/>).</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="columnAlias">The alias for the column (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> Aggregate<TValue>(
        Expression<Func<TEntity, TValue>> property,
        AggregateFunctions function,
        string tableAlias = null,
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Aggregate(function, propertyAccessor.Column, tableAlias, columnAlias);
    }

    /// <summary>
    /// Sets the target table for the SELECT statement using the entity's mapping information by default.
    /// </summary>
    /// <param name="tableName">The name of the table (optional, defaults to entity mapping).</param>
    /// <param name="tableSchema">The schema of the table (optional, defaults to entity mapping).</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public override SelectEntityBuilder<TEntity> From(
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
    /// Adds a JOIN clause to the SELECT statement using the specified builder action for the right entity.
    /// </summary>
    /// <typeparam name="TRight">The type of the right join entity.</typeparam>
    /// <param name="builder">An action that configures the join using a <see cref="JoinEntityBuilder{TEntity, TRight}"/>.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> Join<TRight>(Action<JoinEntityBuilder<TEntity, TRight>> builder)
        where TRight : class
    {
        var innerBuilder = new JoinEntityBuilder<TEntity, TRight>(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return this;
    }

    /// <summary>
    /// Adds a JOIN clause to the SELECT statement using the specified builder action for the left and right entities.
    /// </summary>
    /// <typeparam name="TLeft">The type of the left join entity.</typeparam>
    /// <typeparam name="TRight">The type of the right join entity.</typeparam>
    /// <param name="builder">An action that configures the join using a <see cref="JoinEntityBuilder{TLeft, TRight}"/>.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> Join<TLeft, TRight>(Action<JoinEntityBuilder<TLeft, TRight>> builder)
        where TLeft : class
        where TRight : class
    {
        var innerBuilder = new JoinEntityBuilder<TLeft, TRight>(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return this;
    }

    /// <inheritdoc />
    public SelectEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        return Where<TValue>(property, parameterValue, null, filterOperator);
    }

    /// <inheritdoc />
    public SelectEntityBuilder<TEntity> Where<TValue>(
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
    public SelectEntityBuilder<TEntity> Where<TModel, TValue>(
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
    public SelectEntityBuilder<TEntity> WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        return WhereIf(property, parameterValue, null, filterOperator, condition);
    }

    /// <inheritdoc />
    public SelectEntityBuilder<TEntity> WhereIf<TValue>(
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
    public SelectEntityBuilder<TEntity> WhereIn<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereIn(propertyAccessor?.Column, parameterValues, tableAlias);
    }

    /// <inheritdoc />
    public SelectEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, condition);
    }

    /// <inheritdoc />
    public SelectEntityBuilder<TEntity> WhereInIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        IEnumerable<TValue> parameterValues,
        string tableAlias,
        Func<string, IEnumerable<TValue>, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return WhereInIf(propertyAccessor?.Column, parameterValues, tableAlias, condition);
    }

    /// <inheritdoc />
    public SelectEntityBuilder<TEntity> WhereOr(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.Or);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null)
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }

    /// <inheritdoc />
    public SelectEntityBuilder<TEntity> WhereAnd(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.And);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null && statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }

    /// <summary>
    /// Adds an ORDER BY clause with the specified entity property and sort direction.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to sort by.</param>
    /// <param name="sortDirection">The sort direction (default is <see cref="SortDirections.Ascending"/>).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> OrderBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderBy(propertyAccessor.Column, null, sortDirection);
    }

    /// <summary>
    /// Adds an ORDER BY clause with the specified entity property, sort direction, and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to sort by.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="sortDirection">The sort direction (default is <see cref="SortDirections.Ascending"/>).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> OrderBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderBy(propertyAccessor.Column, tableAlias, sortDirection);
    }

    /// <summary>
    /// Conditionally adds an ORDER BY clause with the specified entity property and sort direction.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to sort by.</param>
    /// <param name="sortDirection">The sort direction (default is <see cref="SortDirections.Ascending"/>).</param>
    /// <param name="condition">A function that determines whether to add the ORDER BY clause, based on the property name. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> OrderByIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        SortDirections sortDirection = SortDirections.Ascending,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderByIf(propertyAccessor.Column, null, sortDirection, condition);
    }

    /// <summary>
    /// Conditionally adds an ORDER BY clause with the specified entity property, sort direction, and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to sort by.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <param name="sortDirection">The sort direction (default is <see cref="SortDirections.Ascending"/>).</param>
    /// <param name="condition">A function that determines whether to add the ORDER BY clause, based on the property name. If <c>null</c>, the clause is always added.</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> OrderByIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias,
        SortDirections sortDirection = SortDirections.Ascending,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderByIf(propertyAccessor.Column, tableAlias, sortDirection, condition);
    }

    /// <summary>
    /// Adds a GROUP BY clause with the specified entity property and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="property">An expression selecting the property to group by.</param>
    /// <param name="tableAlias">The alias of the table (optional).</param>
    /// <returns>
    /// The same builder instance for method chaining.
    /// </returns>
    public SelectEntityBuilder<TEntity> GroupBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return GroupBy(propertyAccessor.Column, tableAlias);
    }

    /// <summary>
    /// Builds the SQL SELECT statement using the current configuration.
    /// </summary>
    /// <returns>
    /// A <see cref="QueryStatement"/> containing the SQL SELECT statement and its parameters.
    /// </returns>
    public override QueryStatement BuildStatement()
    {
        // add table and schema from attribute if not set
        if (FromExpressions.Count == 0)
            From(_typeAccessor.TableName, _typeAccessor.TableSchema);

        return base.BuildStatement();
    }
}
