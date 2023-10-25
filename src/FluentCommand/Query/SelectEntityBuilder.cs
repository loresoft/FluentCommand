using System.Linq.Expressions;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

/// <summary>
/// Select query builder
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
    /// <param name="queryGenerator">The query generator.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <param name="logicalOperator">The logical operator.</param>
    public SelectEntityBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

    /// <summary>
    /// Adds a column expression with the specified property.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <returns></returns>
    public SelectEntityBuilder<TEntity> Column(
        Expression<Func<TEntity, object>> property,
        string tableAlias = null,
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        // alais column as property name if don't match
        if (propertyAccessor.Name == propertyAccessor.Column)
            return Column(propertyAccessor.Column, tableAlias, columnAlias);
        else
            return Column(propertyAccessor.Column, tableAlias, columnAlias ?? propertyAccessor.Name);

    }

    /// <summary>
    /// Adds a column expression with the specified property.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <returns></returns>
    public SelectEntityBuilder<TEntity> Column<TModel>(
        Expression<Func<TModel, object>> property,
        string tableAlias = null,
        string columnAlias = null) where TModel : class
    {
        var typeAccessor = TypeAccessor.GetAccessor<TModel>();
        var propertyAccessor = typeAccessor.FindProperty(property);

        // alais column as property name if don't match
        if (propertyAccessor.Name == propertyAccessor.Column)
            return Column(propertyAccessor.Column, tableAlias, columnAlias);
        else
            return Column(propertyAccessor.Column, tableAlias, columnAlias ?? propertyAccessor.Name);

    }

    /// <summary>
    /// Conditionally adds a column expression with the specified property.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <param name="condition">The condition.</param>
    /// <returns></returns>
    public SelectEntityBuilder<TEntity> ColumnIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null,
        string columnAlias = null,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        // alais column as property name if don't match
        if (propertyAccessor.Name == propertyAccessor.Column)
            return ColumnIf(propertyAccessor.Column, tableAlias, columnAlias, condition);
        else
            return ColumnIf(propertyAccessor.Column, tableAlias, columnAlias ?? propertyAccessor.Name, condition);
    }

    /// <summary>
    /// Adds a column expression for each of specified names.
    /// </summary>
    /// <param name="columnNames">The column names.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">columnNames</exception>
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

            // alias column as property name if don't match
            if (propertyAccessor.Name == propertyAccessor.Column)
                Column(propertyAccessor.Column, tableAlias);
            else
                Column(propertyAccessor.Column, tableAlias, propertyAccessor.Name);
        }

        return this;
    }

    /// <summary>
    /// Adds a column expression for each property in <typeparamref name="TEntity" />.
    /// </summary>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="filter">An optional filter to include properties.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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

            // alias column as property name if don't match
            if (property.Name == property.Column)
                Column(property.Column, tableAlias);
            else
                Column(property.Column, tableAlias, property.Name);
        }

        return this;
    }

    /// <summary>
    /// Adds a column expression for each property in <typeparamref name="TModel" />.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="filter">An optional filter to include properties.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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

            // alias column as property name if don't match
            if (property.Name == property.Column)
                Column(property.Column, tableAlias);
            else
                Column(property.Column, tableAlias, property.Name);
        }

        return this;
    }

    /// <summary>
    /// Adds a count expression using the specified property.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Adds an aggregate expression using the specified function and property.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="function">The aggregate function.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="columnAlias">The column alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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

    /// <inheritdoc />
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
    /// Add a join clause using the specified builder action
    /// </summary>
    /// <typeparam name="TRight">The right join entity</typeparam>
    /// <param name="builder">The join builder.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Add a join clause using the specified builder action
    /// </summary>
    /// <typeparam name="TLeft">The left join entity</typeparam>
    /// <typeparam name="TRight">The right join entity</typeparam>
    /// <param name="builder">The join builder.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Create a where clause with the specified property, value, operator and table alias
    /// </summary>
    /// <typeparam name="TModel">The type of the model</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="parameterValue">The parameter value.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="filterOperator">The filter operator.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Add an order by clause with the specified property and sort direction.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public SelectEntityBuilder<TEntity> OrderBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderBy(propertyAccessor.Column, null, sortDirection);
    }

    /// <summary>
    /// Add an order by clause with the specified property, sort direction and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Conditionally add an order by clause with the specified property and sort direction.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Conditionally add an order by clause with the specified property, sort direction and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <param name="sortDirection">The sort direction.</param>
    /// <param name="condition">The condition.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
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
    /// Add a group by clause with the specified property  and table alias.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="property">The property.</param>
    /// <param name="tableAlias">The table alias.</param>
    /// <returns>
    /// The same builder so that multiple calls can be chained.
    /// </returns>
    public SelectEntityBuilder<TEntity> GroupBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return GroupBy(propertyAccessor.Column, tableAlias);
    }

    /// <inheritdoc />
    public override QueryStatement BuildStatement()
    {
        // add table and schema from attribute if not set
        if (FromExpressions.Count == 0)
            From(_typeAccessor.TableName, _typeAccessor.TableSchema);

        return base.BuildStatement();
    }



}
