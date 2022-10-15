using System.Linq.Expressions;

using FluentCommand.Extensions;
using FluentCommand.Query.Generators;
using FluentCommand.Reflection;

namespace FluentCommand.Query;

public class SelectEntityBuilder<TEntity>
    : SelectBuilder<SelectEntityBuilder<TEntity>>, IWhereEntityBuilder<TEntity, SelectEntityBuilder<TEntity>>
    where TEntity : class
{
    private static readonly TypeAccessor _typeAccessor = TypeAccessor.GetAccessor<TEntity>();

    public SelectEntityBuilder(IQueryGenerator queryGenerator, List<QueryParameter> parameters, LogicalOperators logicalOperator = LogicalOperators.And)
        : base(queryGenerator, parameters, logicalOperator)
    {
    }

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


    public SelectEntityBuilder<TEntity> Count<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null,
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Count(propertyAccessor.Column, tableAlias, columnAlias);
    }

    public SelectEntityBuilder<TEntity> Aggregate<TValue>(
        Expression<Func<TEntity, TValue>> property,
        AggregateFunctions function,
        string tableAlias = null,
        string columnAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Aggregate(function, propertyAccessor.Column, tableAlias, columnAlias);
    }

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

    public SelectEntityBuilder<TEntity> Join<TRight>(Action<JoinEntityBuilder<TEntity, TRight>> builder)
        where TRight : class
    {
        var innerBuilder = new JoinEntityBuilder<TEntity, TRight>(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return this;
    }

    public SelectEntityBuilder<TEntity> Join<TLeft, TRight>(Action<JoinEntityBuilder<TLeft, TRight>> builder)
        where TLeft : class
        where TRight : class
    {
        var innerBuilder = new JoinEntityBuilder<TLeft, TRight>(QueryGenerator, Parameters);
        builder(innerBuilder);

        JoinExpressions.Add(innerBuilder.BuildExpression());

        return this;
    }

    public SelectEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        return Where<TValue>(property, parameterValue, null, filterOperator);
    }

    public SelectEntityBuilder<TEntity> Where<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        string tableAlias,
        FilterOperators filterOperator = FilterOperators.Equal)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return Where(propertyAccessor.Column, parameterValue, tableAlias, filterOperator);
    }

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

    public SelectEntityBuilder<TEntity> WhereIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        TValue parameterValue,
        FilterOperators filterOperator = FilterOperators.Equal,
        Func<string, TValue, bool> condition = null)
    {
        return WhereIf(property, parameterValue, null, filterOperator, condition);
    }

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

    public SelectEntityBuilder<TEntity> WhereOr(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.Or);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null)
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }

    public SelectEntityBuilder<TEntity> WhereAnd(Action<LogicalEntityBuilder<TEntity>> builder)
    {
        var innerBuilder = new LogicalEntityBuilder<TEntity>(QueryGenerator, Parameters, LogicalOperators.And);

        builder(innerBuilder);

        var statement = innerBuilder.BuildStatement();

        if (statement != null || statement.Statement.HasValue())
            WhereExpressions.Add(new WhereExpression(statement.Statement, IsRaw: true));

        return this;
    }


    public SelectEntityBuilder<TEntity> OrderBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderBy(propertyAccessor.Column, null, sortDirection);
    }

    public SelectEntityBuilder<TEntity> OrderBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias,
        SortDirections sortDirection = SortDirections.Ascending)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderBy(propertyAccessor.Column, tableAlias, sortDirection);
    }

    public SelectEntityBuilder<TEntity> OrderByIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        SortDirections sortDirection = SortDirections.Ascending,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderByIf(propertyAccessor.Column, null, sortDirection, condition);
    }

    public SelectEntityBuilder<TEntity> OrderByIf<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias,
        SortDirections sortDirection = SortDirections.Ascending,
        Func<string, bool> condition = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return OrderByIf(propertyAccessor.Column, tableAlias, sortDirection, condition);
    }

    public SelectEntityBuilder<TEntity> GroupBy<TValue>(
        Expression<Func<TEntity, TValue>> property,
        string tableAlias = null)
    {
        var propertyAccessor = _typeAccessor.FindProperty(property);

        return GroupBy(propertyAccessor.Column, tableAlias);
    }

    public override QueryStatement BuildStatement()
    {
        // add table and schema from attribute if not set
        if (FromExpressions.Count == 0)
            From(_typeAccessor.TableName, _typeAccessor.TableSchema);

        return base.BuildStatement();
    }



}
